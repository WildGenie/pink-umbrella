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
using Newtonsoft.Json;
using seattle.Models;
using seattle.Services;
using seattle.ViewModels.Account;
using seattle.Util;

namespace seattle.Controllers
{
    public class AccountController: BaseController
    {
        public AccountController(
                IWebHostEnvironment environment,
                UserManager<UserProfileModel> userManager,
                SignInManager<UserProfileModel> signInManager,
                IPostService posts, IUserProfileService userProfiles
                ) :
            base(environment, signInManager, userManager, posts, userProfiles)
        {
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();

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
                // await eventLog.Log((await GetCurrentUserAsync()).Id, EventNames.Account.LockedOut, null);
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
            // await eventLog.Log((await GetCurrentUserAsync()).Id, EventNames.Account.Logout, null);
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
            // await eventLog.Log(-1, EventNames.Account.Register.Begin, null);
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
                // TODO: data download history
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

                // await eventLog.Log(int.Parse(userId), EventNames.Account.Delete, null);

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
        public IActionResult PersonalData()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(PersonalData);
            // TODO: data download history
            return View(new PersonalDataModel()
            {

            });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(ChangePassword);
            // TODO: data download history
            return View(new ChangePasswordViewModel()
            {

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
            // await eventLog.Log(user.Id, EventNames.Account.ChangePassword, null);
            // StatusMessage = "Your password has been changed.";

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
                // await eventLog.Log(user.Id, EventNames.Account.ResetPassword, null);
                return RedirectToAction(nameof(ResetPasswordConfirm));
            }
            return View();
        }

        public IActionResult ResetPasswordConfirm() => View();

        public async Task<IActionResult> DownloadPersonalData()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(DownloadPersonalData);
            // TODO: data download history
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // await eventLog.Log(user.Id, EventNames.Account.DownloadPersonalData, null);

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
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Register);
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = _userProfiles.CreateUser(input);
                var result = await _userManager.CreateAsync(user, input.Password);
                if (result.Succeeded)
                {
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
                        //await _emailSender.SendEmailAsync(input.Email, "Seattle: Confirm your email",
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

            // await eventLog.Log(-1, EventNames.Account.Register.Error, null);
            // If we got this far, something failed, redisplay form
            ViewData["ReturnUrl"] = returnUrl;
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
            user.DisplayName = MyProfile.DisplayName;
            user.Handle = MyProfile.Handle;
            user.Bio = MyProfile.Bio;
            user.WhenLastLoggedInVisibility = MyProfile.WhenLastLoggedInVisibility.Min(user.Visibility);
            user.WhenLastOnlineVisibility = MyProfile.WhenLastOnlineVisibility.Min(user.Visibility);
            user.BioVisibility = MyProfile.BioVisibility.Min(user.Visibility);
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}