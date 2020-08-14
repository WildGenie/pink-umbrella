using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Developer;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Util;
using PinkUmbrella.Services.Local;
using System.ComponentModel.DataAnnotations;
using Tides.Models;
using Tides.Models.Auth;
using Estuary.Services;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerDeveloper), AllowAnonymous, IsDevOrDebuggingOrElse404Filter]
    public class DeveloperController : BaseController
    {
        private readonly ILogger<DeveloperController> _logger;
        private readonly IDebugService _debugService;
        private readonly RoleManager<UserGroupModel> _roleManager;
        private readonly INotificationService _notifications;

        public DeveloperController(
            IWebHostEnvironment environment,
            ILogger<DeveloperController> logger,
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IPostService posts,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            IDebugService debugService,
            RoleManager<UserGroupModel> roleManager,
            IReactionService reactions, 
            ITagService tags,
            INotificationService notifications,
            IPeerService peers,
            IAuthService auth,
            ISettingsService settings,
            IActivityStreamRepository activityStreams):
            base(signInManager, userManager, localProfiles, publicProfiles, auth, settings)
        {
            _logger = logger;
            _debugService = debugService;
            _roleManager = roleManager;
            _notifications = notifications;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            ViewData["Controller"] = "Developer";
            ViewData["Action"] = nameof(Index);
            return View(new IndexViewModel() {
                MyProfile = user,
            });
        }

        public async Task<IActionResult> Users()
        {
            var user = await GetCurrentUserAsync();
            ViewData["Controller"] = "Developer";
            ViewData["Action"] = nameof(Users);
            return View(new UsersViewModel() {
                MyProfile = user,
                MostRecentlyCreatedUsers = await _localProfiles.GetMostRecentlyCreatedUsers(),
            });
        }

        public async Task<IActionResult> Exceptions(PaginationModel pagination)
        {
            var user = await GetCurrentUserAsync();
            ViewData["Controller"] = "Developer";
            ViewData["Action"] = nameof(Exceptions);
            return View(new ExceptionsViewModel() {
                MyProfile = user,
                Exceptions = await _debugService.Get(pagination)
            });
        }

        public IActionResult ThrowException() => throw new Exception("You threw this exception");

        public async Task<IActionResult> Posts()
        {
            var user = await GetCurrentUserAsync();
            ViewData["Controller"] = "Developer";
            ViewData["Action"] = nameof(Posts);
            return View(new PostsViewModel() {
                MyProfile = user,
                // MostReportedPosts = await _posts.GetMostReportedPosts(),
                // MostBlockedPosts = await _posts.GetMostBlockedPosts(),
                // MostDislikedPosts = await _posts.GetMostDislikedPosts(),
            });
        }

        [HttpGet]
        public async Task<IActionResult> NotificationsForUser(int userId, int? sinceId = null, bool includeViewed = true, PaginationModel pagination = null)
        {
            var user = await GetCurrentUserAsync();
            ViewData["Controller"] = "Account";
            ViewData["Action"] = nameof(NotificationsForUser);

            return View(new NotificationsViewModel()
            {
                MyProfile = user,
                Items = await _notifications.GetNotifications(userId, sinceId, includeViewed, pagination ?? new PaginationModel()),
                SinceId = sinceId,
                IncludeViewed = includeViewed,
            });
        }

        [HttpGet]
        public async Task<IActionResult> Integrations()
        {
            var user = await GetCurrentUserAsync();
            ViewData["Controller"] = "Developer";
            ViewData["Action"] = nameof(Integrations);

            return View(new IntegrationsViewModel()
            {
                MyProfile = user,
                Items = await _auth.GetApiKeys(),
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddApiKey([Required] string clientPublicKey, AuthType authType)
        {
            if (string.IsNullOrWhiteSpace(clientPublicKey))
            {
                ModelState.AddModelError(nameof(clientPublicKey), "Required");
            }
            clientPublicKey =_auth.CleanupKeyString(clientPublicKey);
            var authFormat = AuthKeyFormatResolver.Resolve(clientPublicKey);
            if (!authFormat.HasValue)
            {
                ModelState.AddModelError(nameof(clientPublicKey), "Invalid format");
            }
            if (ModelState.IsValid)
            {
                var pub = await _auth.GetOrAdd(new PublicKey() { Value = clientPublicKey, Type = authType, Format = authFormat.Value });
                var existing = await _auth.GetApiKey(pub);
                if (existing == null)
                {
                    var apiKey = await _auth.GenApiKey(pub, HandshakeMethod.Default);
                }
            }
            return RedirectToAction(nameof(Integrations));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApiKey(long clientPublicKeyId)
        {
            await _auth.DeleteApiKey(clientPublicKeyId);
            return RedirectToAction(nameof(Integrations));
        }

        [HttpGet]
        public async Task<IActionResult> MakeMeDevAndAdmin()
        {
            var user = await GetCurrentLocalUserAsync();
            foreach (var g in new GroupType [] { GroupType.Dev, GroupType.Admin })
            {
                var gname = g.ToString().ToLower();
                if (!await _roleManager.RoleExistsAsync(gname))
                {
                    var result = await _roleManager.CreateAsync(new UserGroupModel() { Name = gname, OwnerId = -1, GroupType = g });
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Cannot create {gname} group");
                    }
                }
                if (!await _userManager.IsInRoleAsync(user, gname))
                {
                    await _userManager.AddToRoleAsync(user, gname);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
