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

        public HomeController(IWebHostEnvironment environment, ILogger<HomeController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IFeedService feeds, IUserProfileService userProfiles):
            base(environment, signInManager, userManager, feeds, userProfiles)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(new IndexViewModel() {
                MyProfile = await _userProfiles.GetUser(0),
                MyFeed = await _feeds.GetFeedForUser(0, 0, false, new PaginationModel() { count = 10, start = 0 })
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
