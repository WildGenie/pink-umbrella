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
using seattle.ViewModels.Profile;

namespace seattle.Controllers
{
    public class ProfileController: BaseController
    {
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IWebHostEnvironment environment, ILogger<ProfileController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IFeedService feeds, IUserProfileService userProfiles):
            base(environment, signInManager, userManager, feeds, userProfiles)
        {
            _logger = logger;
        }
        
        [Route("/Profile/{id?}")]
        public async Task<IActionResult> Index(int? id = null)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Index);
            var user = id.HasValue ? await _userProfiles.GetUser(id.Value) : await GetCurrentUserAsync();
            return View(new IndexViewModel() {
                Profile = user,
                Feed = await _feeds.GetFeedForUser(user.Id, user.Id, false, new PaginationModel() { count = 10, start = 0 })
            });
        }

        public async Task<IActionResult> ProfileByHandle(string handle)
        {
            var user = await _userProfiles.GetUser(handle);
            return RedirectToAction(nameof(Index), new { Id = user.Id });
        }
    }
}