using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Developer;
using PinkUmbrella.Models.AhPushIt;

namespace PinkUmbrella.Controllers
{
    public class DeveloperController : BaseController
    {
        private readonly ILogger<DeveloperController> _logger;
        private readonly IDebugService _debugService;
        private readonly RoleManager<UserGroupModel> _roleManager;

        public DeveloperController(IWebHostEnvironment environment, ILogger<DeveloperController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IDebugService debugService,
            RoleManager<UserGroupModel> roleManager, IReactionService reactions, ITagService tags, INotificationService notifications):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications)
        {
            _logger = logger;
            _debugService = debugService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Developer";
                ViewData["Action"] = nameof(Index);
                return View(new IndexViewModel() {
                    MyProfile = user,
                    UnusedUnexpiredAccessCodes = await _userProfiles.GetUnusedUnexpiredAccessCodes(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Users()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Developer";
                ViewData["Action"] = nameof(Users);
                return View(new UsersViewModel() {
                    MyProfile = user,
                    MostRecentlyCreatedUsers = await _userProfiles.GetMostRecentlyCreatedUsers(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Exceptions(PaginationModel pagination)
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Developer";
                ViewData["Action"] = nameof(Exceptions);
                return View(new ExceptionsViewModel() {
                    MyProfile = user,
                    Exceptions = await _debugService.Get(pagination)
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> ThrowException()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                throw new Exception("You threw this exception");
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Posts()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Developer";
                ViewData["Action"] = nameof(Posts);
                return View(new PostsViewModel() {
                    MyProfile = user,
                    MostReportedPosts = await _posts.GetMostReportedPosts(),
                    MostBlockedPosts = await _posts.GetMostBlockedPosts(),
                    MostDislikedPosts = await _posts.GetMostDislikedPosts(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Community()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Developer";
                ViewData["Action"] = nameof(Community);
                return View();
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiveAccessToGroup()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                return View();
            }
            else
            {
                return Redirect("/Error/404");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> GiveAccessToGroup(int toUserId, string group)
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                if (await _roleManager.RoleExistsAsync(group))
                {
                    var code = await _userProfiles.NewGroupAccessCode(user.Id, toUserId, group);
                    return Content($"You have given {toUserId} access to {group}. The link is\n<a href=\"/AddMeToGroup/{code.Code}\">{code.Code}</a>");
                }
            }
            
            return Redirect("/Error/404");
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(int postId, string group)
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                int[] recipients = null;
                if (group == "*")
                {
                    recipients = _userManager.Users.Select(u => u.Id).ToArray();
                }
                else if (await _roleManager.RoleExistsAsync(group))
                {
                    recipients = (await _userManager.GetUsersInRoleAsync(group)).Select(u => u.Id).ToArray();
                }

                await _notifications.Publish(new Notification() {
                    FromUserId = user.Id,
                    Priority = NotificationPriority.Normal,
                    Type = NotificationType.DIRECT_NOTIFICATION,
                    Subject = ReactionSubject.Post,
                    SubjectId = postId,
                }, recipients);
                return Content($"You have sent {postId} to {group} ({recipients.Length} recipients). ");
            }
            
            return Redirect("/Error/404");
        }

        [HttpGet]
        public async Task<IActionResult> NotificationsForUser(int userId, int? sinceId = null, bool includeViewed = true, bool includeDismissed = false, PaginationModel pagination = null)
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Account";
                ViewData["Action"] = nameof(NotificationsForUser);

                return View(new NotificationsViewModel()
                {
                    MyProfile = user,
                    Items = await _notifications.GetNotifications(userId, sinceId, includeViewed, includeDismissed, pagination ?? new PaginationModel()),
                    SinceId = sinceId,
                    IncludeViewed = includeViewed,
                    IncludeDismissed = includeDismissed,
                });
            }
            
            return Redirect("/Error/404");
        }
    }
}
