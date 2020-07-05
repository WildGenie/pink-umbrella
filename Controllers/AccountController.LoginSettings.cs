using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkUmbrella.ViewModels.Account;
using Microsoft.AspNetCore.Http;
using System.Linq;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Util;

namespace PinkUmbrella.Controllers
{
    public partial class AccountController: BaseController
    {

        [HttpGet]
        public async Task<IActionResult> LoginSettings()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(LoginSettings);

            var user = await GetCurrentUserAsync();

            return View(new LoginSettingsViewModel()
            {
                MyProfile = user,
                MethodOverrides = (await _auth.GetOverriddenLoginMethodsForUser(user.Id)).ToDictionary(k => k.Method, v => v),
                GetMethodDefault = _auth.GetMethodDefault,
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

        [HttpGet]
        public async Task<IActionResult> SetupLoginMethod(UserLoginMethod method, string ReturnUrl)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(SetupLoginMethod);

            var user = await GetCurrentUserAsync();

            return View(new SetupLoginMethodViewModel()
            {
                MyProfile = user,
                Allowed = await _auth.LoginMethodAllowed(user.Id, method, _auth.GetMethodDefault(method)),
                AllowedByDefault = _auth.GetMethodDefault(method),
                Method = method,
                ReturnUrl = ReturnUrl,
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLoginMethod(UserLoginMethod method, bool enabled, string ReturnUrl)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(SetupLoginMethod);

            var user = await GetCurrentUserAsync();

            var result = await _auth.UpdateLoginMethodAllowed(user.Id, method, enabled, _auth.GetMethodDefault(method));
            switch (result.Result)
            {
                case UpdateLoginMethodResult.ResultType.NoError:
                return RedirectToAction(nameof(LoginSettings));
                case UpdateLoginMethodResult.ResultType.NotAllowed:
                ModelState.AddModelError(nameof(enabled), $"{method.GetDisplayName()} cannot be changed");
                break;
                case UpdateLoginMethodResult.ResultType.MinimumOneAllowedLoginMethod:
                ModelState.AddModelError(nameof(enabled), $"Cannot remove {method.GetDisplayName()}, must have at least one login method");
                break;
            }

            return View(nameof(SetupLoginMethod), new SetupLoginMethodViewModel()
            {
                MyProfile = user,
                Allowed = await _auth.LoginMethodAllowed(user.Id, method, _auth.GetMethodDefault(method)),
                AllowedByDefault = _auth.GetMethodDefault(method),
                Method = method,
                ReturnUrl = ReturnUrl,
            });
        }
    }
}