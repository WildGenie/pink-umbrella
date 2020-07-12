using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PinkUmbrella.ViewModels.Account;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Models.Auth;
using Fido2NetLib.Objects;
using Fido2NetLib;
using System.Linq;
using Fido2NetLib.Development;
using PinkUmbrella.Util;
using PinkUmbrella.Models;

namespace PinkUmbrella.Controllers
{
    public partial class AccountController: BaseController
    {

        [HttpGet, AllowAnonymous, RedirectIfNotAnonymous]
        public IActionResult Login() => View();

        [HttpPost, AllowAnonymous, RedirectIfNotAnonymous]
        public async Task<IActionResult> Login([Bind] LoginViewModel login, string returnUrl)
        {
            returnUrl ??= "/";
            
            var result = await _signInManager.PasswordSignInAsync(login.EmailAddress, login.Password, login.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(login.EmailAddress);
                await _localProfiles.LogIn(user.Id, Request.Host.Value);
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = login.RememberMe });
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(login.EmailAddress);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        return RedirectToAction(nameof(ConfirmEmailRequired));
                    }
                    else if (user.BanExpires != null)
                    {
                        var ban = user.BanExpires.Value;
                        var reason = user.BanReason;

                        if (ban >= DateTime.UtcNow)
                        {
                            user.BanExpires = null;
                            user.BannedByUserId = null;
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            await _signInManager.SignOutAsync();
                            return RedirectToAction(nameof(Banned), new { reason });
                        }
                    }
                    else
                    {
                        return Redirect("~/");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(login);
                }
            }
        }

        [AllowAnonymous, HttpPost, FeatureGate(nameof(FeatureFlags.FunctionUserLoginRecoveryKey))]
        public async Task<IActionResult> LoginViaRecoveryKey(string email, string code)
        {
            var user = (await GetCurrentLocalUserAsync()) ?? (email != null ? await _userManager.FindByEmailAsync(email) : null);
            if (user != null)
            {
                var allowed = await _auth.LoginMethodAllowed(user.Id, UserLoginMethod.RecoveryKey, _auth.GetMethodDefault(UserLoginMethod.RecoveryKey));
                if (allowed)
                {
                    var codes = await _auth.GetRecoveryKeys(user.Id);
                    var key = codes.FirstOrDefault(c => c.Code == code);
                    
                    if (code != null)
                    {
                        key.WhenUsed = DateTime.UtcNow;
                        await _auth.SaveAsync();

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return Redirect("~/");
                    }
                }
            }

            return RedirectToAction(nameof(ForgotPassword));
        }

        [HttpGet, AllowAnonymous, RedirectIfNotAnonymous]
        public IActionResult LoginViaPublicKey()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(LoginViaPublicKey);
            return View(new LoginViaPublicKeyViewModel());
        }

        [HttpPost, AllowAnonymous, FeatureGate(FeatureFlags.FunctionUserLoginPublicKey)]
        public async Task<IActionResult> GetPublicKeyChallenge(string key, AuthType type)
        {
            var pubkey = await _auth.GetPublicKey(key, type);
            if (pubkey != null)
            {
                var expiration = DateTime.Today.AddMinutes(5);
                var challenge = await _auth.GenChallenge(pubkey, expiration);
                return Json(new {
                    challenge,
                    expiration
                });
            }
            return Json(new {
                error = "Public key not recognized"
            });
        }

        [HttpPost, AllowAnonymous, FeatureGate(FeatureFlags.FunctionUserLoginPublicKey), RedirectIfNotAnonymous]
        public async Task<IActionResult> LoginViaPublicKey(bool newChallenge, string key, string challenge, string answer, AuthType type)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(LoginViaPublicKey);

            if (type == AuthType.None)
            {
                type = _auth.ResolveType(key);
            }

            var pubkey = await _auth.GetPublicKey(key, type);
            if (pubkey != null)
            {
                if (newChallenge)
                {
                    var expiration = DateTime.Today.AddMinutes(5);
                    challenge = Convert.ToBase64String(await _auth.GenChallenge(pubkey, expiration));
                }
                else
                {
                    var privateKey = await _auth.GetPrivateKey(pubkey.Id, type);
                    if (privateKey != null)
                    {
                        var userId = await _auth.GetUserByKey(pubkey);
                        if (userId.HasValue)
                        {
                            var loginResult = await _localProfiles.LoginPublicKeyChallenge(userId.Value, pubkey, privateKey, challenge, answer, _auth.GetHandler(type));
                            if (loginResult.Error == null)
                            {        
                                return Redirect("~/");
                            }
                            else
                            {
                                ModelState.AddModelError(nameof(answer), loginResult.Error.Message);
                            }
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Key", "Invalid key");
            }
            return View(new LoginViaPublicKeyViewModel()
            {
                Key = key,
                Challenge = challenge,
                Answer = answer,
                Type = type
            });
        }

        public async Task<IActionResult> Logout(string returnUrl)
        {
            await _signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Redirect("/");
            }
        }

        [HttpGet, AllowAnonymous, RedirectIfNotAnonymous]
        public async Task<IActionResult> Register(string returnUrl, string code)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Register);

            var invitationOnly = await _featureManager.IsEnabledAsync(nameof(FeatureFlags.FunctionUserRegisterInvitationOnly));
            if (!string.IsNullOrWhiteSpace(code))
            {
                var codeIsValid = await _invitationService.GetAccessCodeAsync(code, null, InvitationType.Register);
                if (codeIsValid == null)
                {
                    if (invitationOnly)
                    {
                        return Redirect("/Error/404");
                    }
                    else
                    {
                        code = null;
                    }
                }
            }
            else if (invitationOnly)
            {
                return Redirect("/Error/404");
            }

            return View(new RegisterViewModel()
            {
                ReturnUrl = returnUrl,
                Code = code,
            });
        }

        [HttpGet]
        public IActionResult Lockout() => View();

        [HttpPost, AllowAnonymous, RedirectIfNotAnonymous]
        public async Task<IActionResult> Register(string returnUrl, string code, [Bind] RegisterInputModel input)
        {
            GroupAccessCodeModel accessCode = null;
            var isInvitationOnly = await _featureManager.IsEnabledAsync(nameof(FeatureFlags.FunctionUserRegisterInvitationOnly));
            if (!string.IsNullOrWhiteSpace(code))
            {
                accessCode = await _invitationService.GetAccessCodeAsync(code, null, InvitationType.Register);
                if (accessCode == null && isInvitationOnly)
                {
                    return Redirect("/Error/404");
                }
            }
            else if (isInvitationOnly)
            {
                return Redirect("/Error/404");
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = await _localProfiles.CreateUser(input, ModelState);
                if (ModelState.IsValid)
                {
                    var result = await _userManager.CreateAsync(user, input.Password);
                    if (result.Succeeded)
                    {
                        await _localProfiles.MakeFirstUserDev(user);
                        if (accessCode != null)
                        {
                            // TODO: send notification that the invite was claimed
                            await _invitationService.ConsumeAccessCodeAsync(user, accessCode);
                        }

                        // await eventLog.Log(user.Id, EventNames.Account.Register.Success, null);
                        var emailCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        if (_environment.IsProduction())
                        {
                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { userId = user.Id, code = emailCode },
                                protocol: Request.Scheme);

                            // TODO: replace with View() of email
                            //await _emailSender.SendEmailAsync(input.Email, "PinkUmbrella: Confirm your email",
                            //$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        }
                        else
                        {
                            var result2 = await _userManager.ConfirmEmailAsync(user, emailCode);
                            if (!result2.Succeeded)
                            {
                                //AddErrors(result2);
                                return View("ConfirmEmail");
                            }
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // await eventLog.Log(-1, EventNames.Account.Register.Error, null);
            // If we got this far, something failed, redisplay form
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Register);
            return View(new RegisterViewModel() {
                ReturnUrl = returnUrl,
                Input = input,
                Code = code,
            });
        }

        // https://github.com/abergs/fido2-net-lib/blob/a87081c1162dad9175483a42907dadf4bd2fc85d/Demo/Controller.cs
        [HttpPost, Authorize(Roles = "dev"), FeatureGate(FeatureFlags.FunctionUserLoginFIDO)]
        public async Task<IActionResult> GetCredentialOptions(string authenticatorAttachment, string attestationType, bool requireResidentKey, string userVerification)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(ChangePassword);

            var user = await GetCurrentLocalUserAsync();

            // 2. Get user existing keys by username
            var existingKeys = await _auth.GetCredentialsForUser(user.Id);

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = requireResidentKey,
                UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
            };

            if (!string.IsNullOrEmpty(authenticatorAttachment))
                authenticatorSelection.AuthenticatorAttachment = authenticatorAttachment.ToEnum<AuthenticatorAttachment>();

            var exts = new AuthenticationExtensionsClientInputs() 
            { 
                Extensions = true, 
                UserVerificationIndex = true, 
                Location = true, 
                UserVerificationMethod = true, 
                BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds 
                { 
                    FAR = float.MaxValue, 
                    FRR = float.MaxValue 
                } 
            };

            var options = _fido2.RequestNewCredential(new Fido2User()
            {
                Id = BitConverter.GetBytes(user.Id),
                DisplayName = user.DisplayName,
                Name = user.UserName,
            }, existingKeys, authenticatorSelection, attestationType.ToEnum<AttestationConveyancePreference>(), exts);

            // 5. return options to client
            return Json(new {
                attestation = options.Attestation.ToString().ToLower(),
                authenticatorSelection = new {
                    authenticatorAttachment = CustomValue(options.AuthenticatorSelection.AuthenticatorAttachment),
                    requireResidentKey = options.AuthenticatorSelection.RequireResidentKey,
                    userVerification = options.AuthenticatorSelection.UserVerification.ToString().ToLower(),
                },
                challenge = options.Challenge,//TrimEnd().Replace('+', '-').Replace('/', '_'),
                excludeCredentials = options.ExcludeCredentials,
                extensions = options.Extensions,
                error = options.ErrorMessage,
                rp = options.Rp,
                status = options.Status,
                user = options.User,
                pubKeyCredParams = options.PubKeyCredParams.Select(k => new {
                    type = "public-key", // https://developer.mozilla.org/en-US/docs/Web/API/PublicKeyCredentialCreationOptions/pubKeyCredParams
                    alg = k.Alg
                }).ToArray(),
                timeout = options.Timeout,
            });
        }

        private static string CustomValue(AuthenticatorAttachment? authenticatorAttachment) => authenticatorAttachment == AuthenticatorAttachment.Platform ? "platform" : "cross-platform";


        [HttpPost, Authorize(Roles = "dev"), FeatureGate(FeatureFlags.FunctionUserLoginFIDO)]
        public async Task<JsonResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse, [FromQuery] CredentialCreateOptions options)
        {
            var user = await GetCurrentLocalUserAsync();
            try
            {
                // 2. Create callback so that lib can verify credential id is unique to this user
                IsCredentialIdUniqueToUserAsyncDelegate callback = async (IsCredentialIdUniqueToUserParams args) =>
                {
                    var users = await _auth.GetUserIdsByCredentialIdAsync(args.CredentialId);
                    if (users.Count > 0)
                        return false;

                    return true;
                };

                // 2. Verify and make the credentials
                var success = await _fido2.MakeNewCredentialAsync(attestationResponse, options, callback);

                // 3. Store the credentials in db
                await _auth.AddCredential(user, new StoredCredential
                {
                    Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
                    PublicKey = success.Result.PublicKey,
                    UserId = BitConverter.GetBytes(user.Id),
                    UserHandle = success.Result.User.Id,
                    SignatureCounter = success.Result.Counter,
                    CredType = success.Result.CredType,
                    RegDate = DateTime.Now,
                    AaGuid = success.Result.Aaguid
                });

                // 4. return "ok" to the client
                return Json(success);
            }
            catch (Exception e)
            {
                return Json(new { Status = "error", ErrorMessage = e.Message });
            }
        }

        [HttpPost, AllowAnonymous, FeatureGate(FeatureFlags.FunctionUserLoginFIDO)]
        public async Task<ActionResult> AssertionOptions([FromForm] string email, [FromForm] string userVerification)
        {
            try
            {
                var user = !string.IsNullOrEmpty(email) ? await _userManager.FindByEmailAsync(email) : null;
                if (user == null)
                {
                    return NotFound();
                }
                
                var existingCredentials = await _auth.GetCredentialsForUser(user.Id);

                var exts = new AuthenticationExtensionsClientInputs()
                { 
                    SimpleTransactionAuthorization = "FIDO", 
                    GenericTransactionAuthorization = new TxAuthGenericArg 
                    { 
                        ContentType = "text/plain", 
                        Content = new byte[] { 0x46, 0x49, 0x44, 0x4F } 
                    }, 
                    UserVerificationIndex = true, 
                    Location = true, 
                    UserVerificationMethod = true 
                };

                // 3. Create options
                var uv = string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>();
                var options = _fido2.GetAssertionOptions(
                    existingCredentials,
                    uv,
                    exts
                );

                // 5. Return options to client
                return Json(new {
                    allowCredentials = options.AllowCredentials,
                    challenge = options.Challenge,
                    extensions = options.Extensions,
                    error = options.ErrorMessage,
                    status = options.Status,
                    timeout = options.Timeout,
                    userVerification = options.UserVerification?.ToString()?.ToLower()
                });
            }
            catch (Exception e)
            {
                return Json(new AssertionOptions { Status = "error", ErrorMessage = e.Message });
            }
        }

        [HttpPost, AllowAnonymous, FeatureGate(FeatureFlags.FunctionUserLoginFIDO)]
        public async Task<JsonResult> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse clientResponse, [FromQuery] AssertionOptions options)
        {
            try
            {
                // 2. Get registered credential from database
                var creds = await _auth.GetCredentialById(clientResponse.Id);

                if (creds == null)
                {
                    throw new Exception("Unknown credentials");
                }

                // 3. Get credential counter from database
                var storedCounter = creds.SignatureCounter;

                // 4. Create callback to check if userhandle owns the credentialId
                IsUserHandleOwnerOfCredentialIdAsync callback = async (args) =>
                {
                    var storedCreds = await _auth.GetCredentialsForUser(BitConverter.ToInt32(args.UserHandle));
                    return storedCreds.Exists(c => c.Id.SequenceEqual(args.CredentialId));
                };

                // 5. Make the assertion
                var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, callback);

                // 6. Store the updated counter
                await _auth.UpdateCredential(BitConverter.ToInt32(res.CredentialId), res.Counter);

                // 7. return OK to client
                return Json(res);
            }
            catch (Exception e)
            {
                return Json(new AssertionVerificationResult { Status = "error", ErrorMessage = e.Message });
            }
        }
    }
}