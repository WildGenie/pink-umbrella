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
using Fido2NetLib;
using Microsoft.AspNetCore.Http;
using QRCoder;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerAccount)]
    public partial class AccountController: BaseController
    {
        private readonly ILogger<AccountController> _logger;

        private readonly IFido2 _fido2;

        private readonly IInvitationService _invitationService;

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
                IFido2 fido2,
                IInvitationService invitationService
                ) :
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications, peers, auth, settings)
        {
            _logger = logger;
            _fido2 = fido2;
            _invitationService = invitationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string statusMessage, string statusType)
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Index);
            ShowStatus(statusMessage, statusType);

            var user = await GetCurrentUserAsync();
            
            // RecurringJob.AddOrUpdate(() => Console.WriteLine("Transparent!"), Cron.Minutely());

            return View(new IndexViewModel()
            {
                MyProfile = user,
                MethodOverrides = (await _auth.GetOverriddenLoginMethodsForUser(user.Id)).ToDictionary(k => k.Method, v => v),
                GetMethodDefault = _auth.GetMethodDefault,
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

        [HttpGet]
        public async Task<IActionResult> PersonalData()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(PersonalData);
            return View(new PersonalDataModel()
            {
                MyProfile = await GetCurrentUserAsync()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Invites(string selected) // string statusMessage, string statusType
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Invites);

            //ShowStatus(statusMessage, statusType);
            var user = await GetCurrentUserAsync();
            
            return View(new InvitesViewModel()
            {
                MyProfile = user,
                InvitesToMe = await _invitationService.GetInvitesToUser(user.Id),
                InvitesFromMe = await _invitationService.GetInvitesFromUser(user.Id),
                Selected = selected,
            });
        }

        [HttpGet]
        public async Task<IActionResult> Invite()
        {
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(Invite);

            var user = await GetCurrentUserAsync();
            
            return View(new InviteViewModel()
            {
                MyProfile = user,
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite([Bind] InviteFormViewModel values)
        {
            var user = await GetCurrentUserAsync();
            if (ModelState.IsValid)
            {
                if (values.DaysValidFor <= 0)
                {
                    ModelState.AddModelError(nameof(values.DaysValidFor), "Invalid number of days");
                }
                else if (string.IsNullOrWhiteSpace(values.Message))
                {
                    ModelState.AddModelError(nameof(values.Message), "Message is required");
                }
                else if (values.Type == InvitationType.Register && !User.IsInRole("admin"))
                {
                    ModelState.AddModelError(string.Empty, "Must be an admin to create group invitations");
                }
                else
                {
                    var code = await _invitationService.NewInvitation(user.Id, values.Type, values.UserIdTo, values.Message, values.DaysValidFor);
                    if (code != null)
                    {
                        return RedirectToAction(nameof(Invites), new { selected = code.Code });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error creating invitation");
                    }
                }
            }
            return View(new InviteViewModel()
            {
                MyProfile = user,
                Values = values
            });
        }

        public async Task<IActionResult> SendInviteViaMethod(HandshakeMethod method, int id)
        {
            var code = await _invitationService.GetAccessCodeAsync(id);
            var model = new SendInviteViaMethodViewModel()
            {
                MyProfile = await GetCurrentUserAsync(),
            };
            switch (method)
            {
                case HandshakeMethod.Email:
                {
                    var subject = string.Empty;
                    var action = "join";
                    switch (code.Type)
                    {
                        case InvitationType.Register:
                        subject = _settings.Site.HostDisplayName;
                        break;
                        case InvitationType.FollowMe:
                        {
                            var user = model.MyProfile.Id == code.CreatedByUserId ? model.MyProfile : await _userManager.FindByIdAsync(code.CreatedByUserId.ToString());
                            subject = $"{user.DisplayName} @{user.Handle}";
                        } break;
                        case InvitationType.AddMeToGroup:
                        subject = code.GroupName;
                        break;
                    }
                    subject = Uri.EscapeDataString($"Invitation to {action} {subject}");
                    var body = Uri.EscapeDataString(code.GroupName);
                    model.Link = $"mailto:recipient@email.com?body={body}&subject={subject}";
                } break;
                case HandshakeMethod.Link:
                    model.Link = $"/Account/AcceptInvite/{code.Code}";
                break;
                case HandshakeMethod.ManualCodeMachine:
                    model.RawString = code.Code;
                break;
                case HandshakeMethod.ManualCodeHuman:
                    if (Guid.TryParse(code.Code, out var asGuid))
                    {
                        model.RawString = string.Join(' ', _auth.ToBiometric(asGuid.ToByteArray()));
                    }
                    else
                    {
                        model.RawString = code.Code;
                    }
                break;
                case HandshakeMethod.QRCode:
                {
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
                    Base64QRCode qrCode = new Base64QRCode(qrCodeData);
                    model.QRCodeImageAsBase64 = qrCode.GetGraphic(20);
                } break;
                default:
                throw new Exception($"Invalid method {method}");
            }
            return View(model);
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

        [HttpPost]
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

        [HttpPost]
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

        [HttpGet, AllowAnonymous, Route("/Account/AcceptInvite/{code}")]
        public async Task<IActionResult> AcceptInvite(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                var user = await GetCurrentUserAsync();
                var accessCode = await _invitationService.GetAccessCodeAsync(code, user?.Id, null);
                if (accessCode != null)
                {
                    var statusMessage = "Accepted invitation";
                    var statusType = "success";
                    switch (accessCode.Type)
                    {
                        case InvitationType.AddMeToGroup:
                        {
                            if (!await _userManager.IsInRoleAsync(user, accessCode.GroupName))
                            {
                                _logger.LogInformation($"Adding {user.Id} ({user.Email}) to {accessCode.GroupName} role");
                                statusMessage = "You are now a part of " + accessCode.GroupName;
                            }
                            else
                            {
                                _logger.LogInformation($"{user.Id} ({user.Email}) was already a part of {accessCode.GroupName} role");
                                statusMessage = "You were already a part of " + accessCode.GroupName;
                            }
                        } break;
                        case InvitationType.FollowMe:
                        {
                            await _reactions.React(user.Id, accessCode.CreatedByUserId, ReactionType.Follow, ReactionSubject.Profile);
                            statusMessage = "You are now following " + accessCode.CreatedByUserId;
                        } break;
                        case InvitationType.Register: return RedirectToAction(nameof(Register), new { code });
                    }
                    // TODO: send notification that the invite was consumed
                    await _invitationService.ConsumeAccessCodeAsync(user, accessCode);
                    return RedirectToAction(nameof(Index), new { statusMessage, statusType });
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

        [HttpGet]
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

        [HttpPost, ValidateAntiForgeryToken]
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