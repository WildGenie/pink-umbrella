using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
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
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Local;
using Tides.Models;
using Estuary.Core;
using Estuary.Services;
using PinkUmbrella.ViewModels.Shared;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerHome)]
    [AllowAnonymous]
    public class HomeController : ActivityStreamController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISearchService _searchService;

        public HomeController(
            IWebHostEnvironment environment,
            ILogger<HomeController> logger,
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IPostService postService,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            ISearchService searchService,
            IReactionService reactions,
            ITagService tags,
            INotificationService notifications,
            IPeerService peers,
            IAuthService auth,
            ISettingsService settings,
            IActivityStreamRepository activityStreams):
            base(environment, signInManager, userManager, postService, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
            _logger = logger;
            _searchService = searchService;
        }

        [Route("/")]
        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel()
            {
                MyProfile = user,
            };
            
            if (user != null)
            {
                model.MyFeed = ListViewModel.Regular(await _activityStreams.Get(new ActivityStreamFilter("inbox")
                {
                    id = user.PublicId
                }));
                model.MyFeed.EmptyViewName = "_EmptyFollowingList";
                return View(model);
            }
            else
            {
                if (_signInManager.IsSignedIn(User))
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View("Welcome", model);
            }
        }


        [Route("/Welcome")]
        public async Task<IActionResult> Welcome()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Welcome);
            return View(new BaseViewModel() { MyProfile = await GetCurrentUserAsync() });
        }

        [Route("/Mentions")]
        public async Task<IActionResult> Mentions()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Mentions);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user,
                MyFeed = ListViewModel.Regular(await _activityStreams.GetAll(new ActivityStreamFilter("inbox")
                {
                    id = user.PublicId
                }))
            };
            model.MyFeed.EmptyViewName = "_EmptyMentionsList";
            return View(nameof(Index), model);
        }

        [Route("/Posts")]
        public async Task<IActionResult> Posts()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Posts);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel()
            {
                MyProfile = user,
                MyFeed = ListViewModel.Regular(await _activityStreams.GetAll(new ActivityStreamFilter("outbox")
                {
                    id = user.PublicId
                }))
            };
            model.MyFeed.EmptyViewModel = "You have not made any posts. Don't be shy!";
            return View(nameof(Index), model);
        }
        
        [Route("/Search")]
        public async Task<IActionResult> Search(string q, SearchResultOrder order = SearchResultOrder.Top,
                                                SearchResultType? t = null, int start = 0, int count = 10,
                                                string[] tags = null)
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Search);
            var pagination = new PaginationModel() { start = start, count = count };
            var user = await GetCurrentUserAsync();
            var request = new SearchRequestModel() {
                text = q, viewerId = user?.UserId,
                type = t, order = order,
                pagination = pagination, tags = tags
            };
            SearchResultsModel results = await _searchService.Search(request);

            return View(new SearchViewModel()
            {
                SearchText = q,
                Order = order,
                Type = t,
                MyProfile = user,
                Results = results
            });
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Notifications(int? sinceId = null, bool includeViewed = true, PaginationModel pagination = null)
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Notifications);
            var user = await GetCurrentUserAsync();
            var notifs = await _activityStreams.GetAll(new ActivityStreamFilter("notifications")
            {
                id = user.PublicId,
                sinceId = sinceId,
                // includeViewed = includeViewed,
            });

            var fromUsers = new Dictionary<string, BaseObject>();

            return View(new NotificationsViewModel()
            {
                MyProfile = user,
                SinceId = sinceId,
                IncludeViewed = includeViewed,
            });
        }

        [Route("/QuickLinks")]
        public async Task<IActionResult> QuickLinks()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(QuickLinks);
            return View(new BaseViewModel() { MyProfile = await GetCurrentUserAsync() });
        }

        [Route("/Privacy")]
        public async Task<IActionResult> Privacy()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Privacy);
            return View(new PrivacyViewModel() { MyProfile = await GetCurrentUserAsync() });
        }

        [Route("/About")]
        public async Task<IActionResult> About()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(About);
            return View(new BaseViewModel() { MyProfile = await GetCurrentUserAsync() });
        }

        [Route("/Terms")]
        public async Task<IActionResult> Terms()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Terms);
            return View(new BaseViewModel() { MyProfile = await GetCurrentUserAsync() });
        }

        [Route("/Error/{code}")]
        public async Task<IActionResult> Error(string code)
        {
            string originalUrl = null;
            #region snippet_StatusCodeReExecute
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature != null)
            {
                originalUrl =
                    statusCodeReExecuteFeature.OriginalPathBase
                    + statusCodeReExecuteFeature.OriginalPath
                    + statusCodeReExecuteFeature.OriginalQueryString;
            }
            #endregion

            var user = await GetCurrentUserAsync();
            return View(new ErrorViewModel()
            {
                MyProfile = user,
                ErrorCode = code,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                OriginalURL = originalUrl,
            });
        }
    }
}
