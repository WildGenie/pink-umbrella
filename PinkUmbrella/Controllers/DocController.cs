using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Home;
using PinkUmbrella.ViewModels;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Services.Local;
using PinkUmbrella.ViewModels.Doc;
using Tides.Services;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerDoc)]
    [AllowAnonymous]
    public class DocController : BaseController
    {
        public DocController(IWebHostEnvironment environment, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService postService, IUserProfileService localProfiles, IPublicProfileService publicProfiles, ISearchService searchService,
            IReactionService reactions, IFeedService feedService, ITagService tags, INotificationService notifications, IPeerService peers,
            IAuthService auth, ISettingsService settings, IActivityStreamRepository activityStreams):
            base(environment, signInManager, userManager, postService, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Doc";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                Source = FeedSource.Following,
                MyProfile = user,
            };
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Api()
        {
            ViewData["Controller"] = "Doc";
            ViewData["Action"] = nameof(Api);
            return View(new BaseViewModel() { MyProfile = await GetCurrentUserAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> Data(string selected)
        {
            ViewData["Controller"] = "Doc";
            ViewData["Action"] = nameof(Data);
            var user = await GetCurrentUserAsync();
            return View(new DataViewModel() { Selected = selected, MyProfile = await GetCurrentUserAsync() });
        }
    }
}
