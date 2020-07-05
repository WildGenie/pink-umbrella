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
using Fido2NetLib;
using Microsoft.AspNetCore.Http;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerAccount)]
    public partial class AccountController: BaseController
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
        public async Task<IActionResult> Index(string statusMessage, string statusType)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Index);
            ShowStatus(statusMessage, statusType);

            var user = await GetCurrentUserAsync();
            
            // RecurringJob.AddOrUpdate(() => Console.WriteLine("Transparent!"), Cron.Minutely());

            return View(new IndexViewModel()
            {
                MyProfile = user
            });
        }

        public IActionResult Google() => View();

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

        [HttpPost, Authorize, FeatureGate(FeatureFlags.FunctionUserAddAuthKey)]
        public async Task<IActionResult> AddAuthKey(string value, AuthType type)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(AddAuthKey);
            var user = await GetCurrentUserAsync();

            var key = new PublicKey()
            {
                Value = value,
                Type = type,
            };
            var result = await _auth.TryAddUserKey(key, user);
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
                    PublicKey = key
                },
            });
        }

        [HttpPost, Authorize, FeatureGate(FeatureFlags.FunctionUserGenAuthKey)]
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

            return RedirectToAction(nameof(Index), new { statusMessage = "Successfully updated account", statusType = "success" });
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
            return RedirectToAction(nameof(Index), new { statusMessage = "Successfully updated profile", statusType = "success" });
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

            ShowStatus("Successfully saved notification settings", "success");

            return View(new NotificationSettingsViewModel()
            {
                MyProfile = user,
                Settings = await _notifications.GetMethodSettings(user.Id)
            });
        }
    }
}