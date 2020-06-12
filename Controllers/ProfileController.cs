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
        
        public async Task<IActionResult> Index(string id)
        {
            var user = id == null ? await GetCurrentUserAsync() : await _userProfiles.GetUser(id);
            return View(new IndexViewModel() {
                Profile = user,
                Feed = await _feeds.GetFeedForUser(user.Id, user.Id, false, new PaginationModel() { count = 10, start = 0 })
            });
        }

        public async Task<IActionResult> ProfileById(int id)
        {
            var user = await _userProfiles.GetUser(id);
            return RedirectToAction(nameof(Index), new { Handle = user.Handle });
        }
    }
}