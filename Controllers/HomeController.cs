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
            var user = await GetCurrentUserAsync();
            return View(new IndexViewModel() {
                MyProfile = user,
                MyFeed = await _feeds.GetFeedForUser(user.Id, user.Id, false, new PaginationModel() { count = 10, start = 0 })
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        public async Task<IActionResult> Search(string q, SearchResultOrder order, SearchResultType? t, int start, int count = 10)
        {
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
