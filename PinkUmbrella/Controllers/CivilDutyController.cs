using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Services.Local;
using Tides.Services;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerCivicDuty)]
    public class CivicDutyController : BaseController
    {
        private readonly ILogger<CivicDutyController> _logger;

        public CivicDutyController(IWebHostEnvironment environment, ILogger<CivicDutyController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService localProfiles, IPublicProfileService publicProfiles, IReactionService reactions, ITagService tags,
            INotificationService notifications, IPeerService peers, IAuthService auth, ISettingsService settings, IActivityStreamRepository activityStreams):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "CivicDuty";
            ViewData["Action"] = nameof(Index);
            return View(new BaseViewModel() {
                MyProfile = await GetCurrentUserAsync()
            });
        }
    }
}
