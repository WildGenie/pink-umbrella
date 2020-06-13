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
    public class DebugController : BaseController
    {
        private readonly ILogger<DebugController> _logger;

        public DebugController(IWebHostEnvironment environment, ILogger<DebugController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles):
            base(environment, signInManager, userManager, posts, userProfiles)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Debug";
            ViewData["Action"] = nameof(Index);
            return View(new IndexViewModel() {
                MyProfile = await GetCurrentUserAsync(),
                MyFeed = await _posts.GetFeedForUser(0, 0, false, new PaginationModel() { count = 10, start = 0 })
            });
        }
    }
}
