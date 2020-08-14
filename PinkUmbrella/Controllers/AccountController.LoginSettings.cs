using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkUmbrella.ViewModels.Account;
using Microsoft.AspNetCore.Http;
using System.Linq;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Util;
using PinkUmbrella.ViewModels.Account.SetupLoginMethod;

namespace PinkUmbrella.Controllers
{
    public partial class AccountController: ActivityStreamController
    {

        // [HttpGet]
        // public async Task<IActionResult> LoginSettings(string statusMessage, string statusType)
        // {
        //     ViewData["Controller"] = "Account";
        //     ViewData["Action"] = nameof(LoginSettings);
        //     ShowStatus(statusMessage, statusType);

        //     var user = await GetCurrentUserAsync();

        //     return View(new LoginSettingsViewModel()
        //     {
        //         MyProfile = user,
        //         MethodOverrides = (await _auth.GetOverriddenLoginMethodsForUser(user.Id)).ToDictionary(k => k.Method, v => v),
        //         GetMethodDefault = _auth.GetMethodDefault,
        //     });
        // }

        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email = null)
        {
            var user = (await GetCurrentLocalUserAsync()) ?? (email != null ? await _userManager.FindByEmailAsync(email) : null);

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

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPasswordEmailSent() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind] ChangePasswordViewModel changePassword)
        {
            var localUser = await GetCurrentLocalUserAsync();
            changePassword.MyProfile = await GetCurrentUserAsync();

            if (!ModelState.IsValid)
            {
                return View(changePassword);
            }

            if (changePassword.MyProfile == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var RequirePassword = await _userManager.HasPasswordAsync(localUser);
            if (RequirePassword)
            {
                if (changePassword.CurrentPassword == changePassword.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Password already set to that.");
                    return View(changePassword);
                }
                else if (!await _userManager.CheckPasswordAsync(localUser, changePassword.CurrentPassword))
                {
                    ModelState.AddModelError(nameof(changePassword.CurrentPassword), "Password is invalid.");
                    return View(changePassword);
                }
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(localUser, changePassword.CurrentPassword, changePassword.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(changePassword);
            }

            await _signInManager.RefreshSignInAsync(localUser);

            return RedirectToAction(nameof(Index), new { statusMessage = "Successfully changed password", statusType = "success" });
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

        [HttpGet]
        public async Task<IActionResult> SetupLoginMethod(UserLoginMethod method, string ReturnUrl)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(SetupLoginMethod);

            var user = await GetCurrentUserAsync();
            var allowed = await _auth.LoginMethodAllowed(user.UserId.Value, method, _auth.GetMethodDefault(method));
            var model = new SetupLoginMethodViewModel()
            {
                MyProfile = user,
                Allowed = allowed,
                AllowedByDefault = _auth.GetMethodDefault(method),
                Method = method,
                ReturnUrl = ReturnUrl,
            };

            if (allowed)
            {
                switch (method)
                {
                    case UserLoginMethod.EmailPassword:
                    model.LoginModel = new ChangePasswordViewModel();
                    break;
                    case UserLoginMethod.RecoveryKey:
                    model.LoginModel = new SetupRecoveryViewModel()
                    {
                        RecoveryKeys = await _auth.GetRecoveryKeys(user.UserId.Value)
                    };
                    break;
                    default: return Redirect("/Error/404");
                }
            }
            // var secretKey = new byte[] {  };
            // var hotp = new Hotp(secretKey, mode: OtpHashMode.Sha512);
            // hotp.VerifyHotp();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLoginMethod(UserLoginMethod method, bool enabled, string ReturnUrl)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(SetupLoginMethod);

            var user = await GetCurrentUserAsync();

            var result = await _auth.UpdateLoginMethodAllowed(user.UserId.Value, method, enabled, _auth.GetMethodDefault(method));
            switch (result.Result)
            {
                case UpdateLoginMethodResult.ResultType.NoError:
                if (enabled)
                {
                    return RedirectToAction(nameof(SetupLoginMethod), new { method });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
                case UpdateLoginMethodResult.ResultType.NotAllowed:
                ModelState.AddModelError(nameof(enabled), $"{method.GetDisplayName()} cannot be changed");
                break;
                case UpdateLoginMethodResult.ResultType.MinimumOneAllowedLoginMethod:
                ModelState.AddModelError(nameof(enabled), $"Cannot remove {method.GetDisplayName()}, must have at least one login method");
                break;
            }

            if (string.IsNullOrWhiteSpace(ReturnUrl))
            {
                return RedirectToAction(nameof(SetupLoginMethod), new { method });
            }
            else
            {
                return Redirect(ReturnUrl);
            }
            // return View(nameof(SetupLoginMethod), new SetupLoginMethodViewModel()
            // {
            //     MyProfile = user,
            //     Allowed = await _auth.LoginMethodAllowed(user.Id, method, _auth.GetMethodDefault(method)),
            //     AllowedByDefault = _auth.GetMethodDefault(method),
            //     Method = method,
            //     ReturnUrl = ReturnUrl,
            // });
        }

        [HttpPost]
        public async Task<IActionResult> ShowRecoveryKey(long id, string ReturnUrl)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(ShowRecoveryKey);

            var user = await GetCurrentUserAsync();
            var codes = await _auth.GetRecoveryKeys(user.UserId.Value);
            var code = codes.SingleOrDefault(c => c.Id == id);
            if (code != null && code.WhenShown == null)
            {
                code.WhenShown = DateTime.UtcNow;
                await _auth.SaveAsync();
            }
            return RedirectToAction(nameof(SetupLoginMethod), new { method = UserLoginMethod.RecoveryKey });
        }

        [HttpPost]
        public async Task<IActionResult> AddRecoveryKey(string label, int? count, int? length, string ReturnUrl)
        {
            if (!string.IsNullOrWhiteSpace(label) && label.Length < 100)
            {
                var user = await GetCurrentUserAsync();
                var code = await _auth.CreateRecoveryKeys(user.UserId.Value, label, length ?? 6, count ?? 1);
                return RedirectToAction(nameof(SetupLoginMethod), new { method = UserLoginMethod.RecoveryKey });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRecoveryKey(long id)
        {
            var user = await GetCurrentUserAsync();
            var codes = await _auth.GetRecoveryKeys(user.UserId.Value);
            var code = codes.FirstOrDefault(c => c.Id == id);
            if (code != null)
            {
                await _auth.DeleteRecoveryKey(code);
            }
            return RedirectToAction(nameof(SetupLoginMethod), new { method = UserLoginMethod.RecoveryKey });
        }
    }
}