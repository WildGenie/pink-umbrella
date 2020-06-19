using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using PinkUmbrella.Services.Sql.Search;

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISearchService _searchService;
        private readonly IFeedService _feedService;

        public HomeController(IWebHostEnvironment environment, ILogger<HomeController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService postService, IUserProfileService userProfiles, ISearchService searchService,
            IReactionService reactions, IFeedService feedService, ITagService tags):
            base(environment, signInManager, userManager, postService, userProfiles, reactions, tags)
        {
            _logger = logger;
            _searchService = searchService;
            _feedService = feedService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                Source = FeedSource.Following,
                MyProfile = user,
            };

            if (user != null)
            {
                model.MyFeed = await _feedService.GetFeedForUser(user.Id, user.Id, false, new PaginationModel() { count = 10, start = 0 });
                return View(model);
            }
            else
            {
                if (_signInManager.IsSignedIn(User)) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View("Welcome", model);
            }
        }

        [Route("/Welcome")]
        public IActionResult Welcome()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Welcome);
            return View();
        }

        [Route("/Mentions")]
        public async Task<IActionResult> Mentions()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Mentions);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                Source = FeedSource.Mentions,
                MyProfile = user,
                MyFeed = await _posts.GetMentionsForUser(user.Id, user.Id, false, new PaginationModel() { count = 10, start = 0 })
            };
            return View(nameof(Index), model);
        }

        [Route("/MyPosts")]
        public async Task<IActionResult> MyPosts()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(MyPosts);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                Source = FeedSource.Myself,
                MyProfile = user,
                MyFeed = await _posts.GetPostsForUser(user.Id, user.Id, false, new PaginationModel() { count = 10, start = 0 })
            };
            return View(nameof(Index), model);
        }

        [Route("/Privacy")]
        public IActionResult Privacy()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Privacy);
            return View(new PrivacyViewModel());
        }

        [Route("/About")]
        public IActionResult About()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(About);
            return View();
        }
        
        [Route("/Search")]
        public async Task<IActionResult> Search(string q, SearchResultOrder order = SearchResultOrder.Top, SearchResultType? t = null, int start = 0, int count = 10)
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Search);
            var pagination = new PaginationModel() { start = start, count = count };
            var user = await GetCurrentUserAsync();
            SearchResultsModel results = await _searchService.Search(q, user?.Id, t, order, pagination);

            return View(new SearchViewModel() {
                SearchText = q,
                Order = order,
                Type = t,
                MyProfile = user,
                Results = results
            });
        }

        [Route("/Error/{code}")]
        public IActionResult Error(string code)
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

            return View(new ErrorViewModel() {
                ErrorCode = code,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                OriginalURL = originalUrl,
            });
        }
    }
}
