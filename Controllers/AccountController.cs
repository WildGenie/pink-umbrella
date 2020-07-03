using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Account;
using PinkUmbrella.Util;
using PinkUmbrella.Models.AhPushIt;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Models.Auth;
using Hangfire;
using Fido2NetLib.Objects;
using Fido2NetLib;
using Fido2NetLib.Development;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerAccount)]
    public class AccountController: BaseController
    {
        private readonly ILogger<AccountController> _logger;

        private readonly IFido2 _fido2;

        public AccountController(
                ILogger<AccountController> logger,
                IWebHostEnvironment environment,
                UserManager<UserProfileModel> userManager,
                SignInManager<UserProfileModel> signInManager,
                IPostService posts, IUserProfileService userProfiles,
                IReactionService reactions, ITagService tags,
                INotificationService notifications,
                IPeerService peers,
                IAuthService auth,
                ISettingsService settings,
                IFido2 fido2
                ) :
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications, peers, auth, settings)
        {
            _logger = logger;
            _fido2 = fido2;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Transparent!"), Cron.Minutely());

            return View(new IndexViewModel()
            {
                MyProfile = user
            });
        }

        public IActionResult Google() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => 
            _signInManager.IsSignedIn(HttpContext.User) ? (IActionResult) Redirect("/") : View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind] LoginViewModel login, string returnUrl)
        {
            returnUrl ??= "/";
            var result = await _signInManager.PasswordSignInAsync(login.EmailAddress, login.Password, login.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(login.EmailAddress);
                await _userProfiles.LogIn(user.Id, Request.Host.Value);
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Register);
            return View(new RegisterViewModel()
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpGet]
        public IActionResult Lockout() => View();

        [HttpGet]
        public IActionResult ConfirmEmailRequired() => View();

        [Authorize]
        public async Task<IActionResult> Delete([Bind] DeleteAccountViewModel password)
        {
            var user = await GetCurrentUserAsync();

            if (HttpContext.Request.Method == "GET")
            {
                ViewData["Controller"] = "Account";
                ViewData["Action"] = nameof(Delete);
                HttpContext.Items["RequirePassword"] = await _userManager.HasPasswordAsync(user);
                ModelState.Clear();
                return View(password ?? new DeleteAccountViewModel());
            }
            else if (HttpContext.Request.Method == "POST")
            {
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var RequirePassword = await _userManager.HasPasswordAsync(user);
                if (RequirePassword)
                {
                    if (password.CurrentPassword != password.ConfirmPassword || !await _userManager.CheckPasswordAsync(user, password.CurrentPassword))
                    {
                        ModelState.AddModelError(string.Empty, "Password not correct.");
                        return View(password);
                    }
                }

                var result = await _userManager.DeleteAsync(user);
                var userId = await _userManager.GetUserIdAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleteing user with ID '{userId}'.");
                }

                await _signInManager.SignOutAsync();

                return Redirect("~/");
            }
            else
            {
                throw new ArgumentOutOfRangeException("Method");
            }
        }

        [HttpGet]
        public IActionResult Banned(string reason)
        {
            ViewData[nameof(reason)] = reason;
            return View();
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> PersonalData()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(PersonalData);
            return View(new PersonalDataModel()
            {
                MyProfile = await GetCurrentUserAsync()
            });
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> AuthKeys()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(AuthKeys);

            var user = await GetCurrentUserAsync();
            return View(new AuthKeysViewModel()
            {
                MyProfile = user,
                Keys = await _auth.GetForUser(user.Id),
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> AddAuthKey(PublicKey AuthKey)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(AddAuthKey);
            var user = await GetCurrentUserAsync();

            var result = await _auth.TryAddUserKey(AuthKey, user);
            switch (result.Error)
            {
                case AuthKeyError.None: return RedirectToAction(nameof(AuthKeys));
                case AuthKeyError.InvalidFormat:
                ModelState.AddModelError("PublicKey", "Invalid format");
                break;
            }

            return View(nameof(AuthKeys), new AuthKeysViewModel()
            {
                MyProfile = user,
                NewKey = new AddKeyViewModel() {
                    PublicKey = AuthKey
                },
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> GenAuthKey(AuthKeyOptions AuthKey)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(GenAuthKey);
            var user = await GetCurrentUserAsync();

            AuthKey.Format = AuthKeyFormat.Raw;
            var result = await _auth.GenKey(AuthKey, HandshakeMethod.Default);
            switch (result.Error)
            {
                case AuthKeyError.None:
                {
                    result = await _auth.TryAddUserKey(result.Keys.Public, user);
                    switch (result.Error)
                    {
                        case AuthKeyError.None: return RedirectToAction(nameof(AuthKeys));
                        case AuthKeyError.InvalidFormat:
                        ModelState.AddModelError("PublicKey", "Invalid format");
                        break;
                    }
                }
                break;
                case AuthKeyError.InvalidFormat:
                ModelState.AddModelError("PublicKey", "Invalid format");
                break;
            }

            return View(nameof(AuthKeys), new AuthKeysViewModel()
            {
                MyProfile = user,
                NewKey = new AddKeyViewModel() {
                    AuthKey = AuthKey
                },
            });
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(ChangePassword);
            return View(new ChangePasswordViewModel()
            {
                MyProfile = await GetCurrentUserAsync()
            });
        }

        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email = null)
        {
            var user = (await GetCurrentUserAsync()) ?? (email != null ? await _userManager.FindByEmailAsync(email) : null);

            if (user == null)
            {
                return View();
            }

            if (HttpContext.Request.Method == "GET" || HttpContext.Request.Method == "POST")
            {
                var confirmationCode = await _userManager.GeneratePasswordResetTokenAsync(user);

                var callbackUrl = Url.Action("ResetPassword", "Security",
                    new { userId = user.Id, code = confirmationCode });

                // await eventLog.Log(user.Id, EventNames.Account.ForgotPassword, null);
                //await _emailSender.SendEmailAsync(email, "Reset your password",
                //$"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToAction(nameof(ForgotPasswordEmailSent));
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordEmailSent() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind] ChangePasswordViewModel changePassword)
        {
            // TODO: data download history
            if (!ModelState.IsValid)
            {
                return View(changePassword);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (changePassword.CurrentPassword == changePassword.ConfirmPassword || !await _userManager.CheckPasswordAsync(user, changePassword.CurrentPassword))
                {
                    ModelState.AddModelError(string.Empty, "Password not correct.");
                    return View(changePassword);
                }
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(changePassword);
            }

            await _signInManager.RefreshSignInAsync(user);

            return Redirect("/Account");
        }

        public ActionResult ResetPassword(string userId, string code)
        {
            if (userId == null || code == null)
            {
                throw new ApplicationException("Code must be ssupplied for password reset");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Code, resetPasswordViewModel.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirm));
            }
            return View();
        }

        public IActionResult ResetPasswordConfirm() => View();

        public async Task<IActionResult> DownloadPersonalData()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(DownloadPersonalData);
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(UserProfileModel).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(personalData)), "text/json");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl, [Bind] RegisterInputModel input)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = await _userProfiles.CreateUser(input, ModelState);
                if (ModelState.IsValid)
                {
                    var result = await _userManager.CreateAsync(user, input.Password);
                    if (result.Succeeded)
                    {
                        await _userProfiles.MakeFirstUserDev(user);

                        // await eventLog.Log(user.Id, EventNames.Account.Register.Success, null);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        if (_environment.IsProduction())
                        {
                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { userId = user.Id, code = code },
                                protocol: Request.Scheme);

                            // TODO: replace with View() of email
                            //await _emailSender.SendEmailAsync(input.Email, "PinkUmbrella: Confirm your email",
                            //$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        }
                        else
                        {
                            var result2 = await _userManager.ConfirmEmailAsync(user, code);
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
            });
        }

        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(ConfirmEmail);
            if (code == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(await _userManager.FindByIdAsync(userId.ToString()), code);
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            //AddErrors(result);
            return View();
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> UpdateAccount([Bind] UserProfileModel MyProfile)
        {
            var user = await GetCurrentUserAsync();
            var emailChanged = user.Email != MyProfile.Email;
            var usernameChanged = user.UserName != MyProfile.UserName && MyProfile.UserName != null;
            var visibilityChanged = user.Visibility != MyProfile.Visibility;

            user.Email = MyProfile.Email;
            user.UserName = MyProfile.UserName ?? user.UserName;
            user.Visibility = MyProfile.Visibility;
            user.WhenLastUpdated = DateTime.UtcNow;

            if (visibilityChanged)
            {
                user.WhenLastLoggedInVisibility = user.WhenLastLoggedInVisibility.Min(user.Visibility);
                user.WhenLastOnlineVisibility = user.WhenLastOnlineVisibility.Min(user.Visibility);
                user.BioVisibility = user.BioVisibility.Min(user.Visibility);
            }

            await _userManager.UpdateAsync(user);

            if (emailChanged)
            {
                var emailConfirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // generate url for page where you can confirm the email
                var callbackurl= "http://example.com/ConfirmEmail";

                // append userId and confirmation code as parameters to the url
                callbackurl += String.Format("?userId={0}&code={1}", user.Id, HttpUtility.UrlEncode(emailConfirmationCode));

                var htmlContent = String.Format(
                        @"Thank you for updating your email. Please confirm the email by clicking this link: 
                        <br><a href='{0}'>Confirm new email</a>",
                        callbackurl);

                // send email to the user with the confirmation link
                //await _userManager.SendEmailAsync(user.Id, subject: "Email confirmation", body: htmlContent);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> UpdateProfile([Bind] UserProfileModel MyProfile)
        {
            var user = await GetCurrentUserAsync();
            if (MyProfile.Handle != user.Handle && await _userProfiles.HandleExists(MyProfile.Handle))
            {
                return BadRequest("Handle already in use");
            }
            user.DisplayName = MyProfile.DisplayName;
            user.Handle = MyProfile.Handle;
            user.Bio = MyProfile.Bio;
            user.WhenLastLoggedInVisibility = MyProfile.WhenLastLoggedInVisibility.Min(user.Visibility);
            user.WhenLastOnlineVisibility = MyProfile.WhenLastOnlineVisibility.Min(user.Visibility);
            user.BioVisibility = MyProfile.BioVisibility.Min(user.Visibility);
            user.WhenLastUpdated = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        [Authorize, Route("/AddMeToGroup/{code}")]
        public async Task<IActionResult> AddMeToGroup(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                var user = await GetCurrentUserAsync();
                var groupAccess = await _userProfiles.GetGroupAccessCodeAsync(code, user.Id);
                if (groupAccess != null)
                {
                    if (!await _userManager.IsInRoleAsync(user, groupAccess.GroupName))
                    {
                        _logger.LogInformation($"Adding {user.Id} ({user.Email}) to {groupAccess.GroupName} role");
                        await _userProfiles.ConsumeGroupAccessCodeAsync(user, groupAccess);
                        return Content("You are now a part of " + groupAccess.GroupName);
                    }
                    else
                    {
                        return Content("You were already a part of " + groupAccess.GroupName);
                    }
                }
            }
            
            return NotFound();
        }

        [AllowAnonymous]
        public async Task<IActionResult> IsHandleUnique([FromQuery(Name="MyProfile.Handle")] string handle_1, [FromQuery(Name="Input.Handle")] string handle_2)
        {
            var handle = handle_1 ?? handle_2;
            if (!string.IsNullOrWhiteSpace(handle))
            {
                var user = await GetCurrentUserAsync();
                if (user?.Handle == handle)
                {
                    return Json(true);
                }
                else
                {
                    return Json(!await _userProfiles.HandleExists(handle));
                }
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> NotificationSettings()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(NotificationSettings);
            var user = await GetCurrentUserAsync();

            return View(new NotificationSettingsViewModel()
            {
                MyProfile = user,
                Settings = await _notifications.GetMethodSettings(user.Id)
            });
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> NotificationSettings(string[] enabledTypeMethods, string submit)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(NotificationSettings);
            var user = await GetCurrentUserAsync();

            if (string.IsNullOrWhiteSpace(submit))
            {
                var typeMethods = new Dictionary<NotificationType, List<NotificationMethod>>();

                foreach (var typeMethod in enabledTypeMethods)
                {
                    var split = typeMethod.Split('-');
                    if (Enum.TryParse(typeof(NotificationType), split[0], true, out var type))
                    {
                        if (Enum.TryParse(typeof(NotificationMethod), split[1], true, out var method))
                        {
                            if (typeMethods.TryGetValue((NotificationType)type, out var methods))
                            {
                                methods.Add((NotificationMethod) method);
                            }
                            else
                            {
                                typeMethods.Add((NotificationType) type, new List<NotificationMethod>() { (NotificationMethod) method });
                            }
                        }
                    }
                }

                await _notifications.UpdateMethodSettings(user.Id, typeMethods);
            }
            else
            {
                if (Enum.TryParse(typeof(NotificationMethod), submit, true, out var method))
                {
                    await _notifications.UpdateMethodSettingsSetAll(user.Id, (NotificationMethod) method);
                }
            }

            return View(new NotificationSettingsViewModel()
            {
                MyProfile = user,
                Settings = await _notifications.GetMethodSettings(user.Id)
            });
        }

        // https://github.com/abergs/fido2-net-lib/blob/a87081c1162dad9175483a42907dadf4bd2fc85d/Demo/Controller.cs
        [HttpPost, Authorize(Roles = "dev"), FeatureGate(FeatureFlags.FunctionUserLoginFIDO)]
        public async Task<IActionResult> GetCredentialOptions(string authenticatorAttachment, string attestationType, bool requireResidentKey, string userVerification)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(ChangePassword);

            var user = await GetCurrentUserAsync();

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
            var user = await GetCurrentUserAsync();
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