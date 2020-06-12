using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using seattle.Models;
using seattle.Services;
using seattle.ViewModels.Home;

namespace seattle.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISearchService _searchService;

        public HomeController(IWebHostEnvironment environment, ILogger<HomeController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IFeedService feeds, IUserProfileService userProfiles, ISearchService searchService):
            base(environment, signInManager, userManager, feeds, userProfiles)
        {
            _logger = logger;
            _searchService = searchService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user,
            };

            if (user != null)
            {
                model.MyFeed = await _feeds.GetFeedForUser(user.Id, user.Id, false, new PaginationModel() { count = 10, start = 0 });
                return View(model);
            }
            else
            {
                if (_signInManager.IsSignedIn(User)) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View("IndexAnonymous", model);
            }
        }

        public IActionResult Privacy()
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Privacy);
            return View();
        }
        
        [Route("/Search")]
        public async Task<IActionResult> Search(string q, SearchResultOrder order, SearchResultType? t, int start, int count = 10)
        {
            ViewData["Controller"] = "Home";
            ViewData["Action"] = nameof(Search);
            var pagination = new PaginationModel() { start = start, count = count };
            var user = await GetCurrentUserAsync();
            SearchResultsModel results;
            if (t.HasValue) {
                results = await _searchService.Get(t.Value).Search(q, order, pagination);
            } else {
                results = await _searchService.Search(q, order, pagination);
            }

            return View(new SearchViewModel() {
                SearchText = q,
                Order = order,
                Type = t,
                MyProfile = user,
                Results = results
            });
        }
    }
}
