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
using PinkUmbrella.ViewModels.Shop;
using PinkUmbrella.ViewModels.Archive;

namespace PinkUmbrella.Controllers
{
    public class ArchiveController : BaseController
    {
        private readonly ILogger<ArchiveController> _logger;
        private readonly IArchiveService _archive;

        public ArchiveController(IWebHostEnvironment environment, ILogger<ArchiveController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IArchiveService archive, IReactionService reactions):
            base(environment, signInManager, userManager, posts, userProfiles, reactions)
        {
            _logger = logger;
            _archive = archive;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Archive";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user
            };

            return View(model);
        }

        public async Task<IActionResult> Image(int id)
        {
            ViewData["Controller"] = "Archive";
            ViewData["Action"] = nameof(Image);
            var user = await GetCurrentUserAsync();
            var model = new ImageViewModel() {
                MyProfile = user,
                Image = await _archive.GetImage(id)
            };

            return View(model);
        }

        public async Task<IActionResult> Video(int id)
        {
            ViewData["Controller"] = "Archive";
            ViewData["Action"] = nameof(Video);
            var user = await GetCurrentUserAsync();
            var model = new VideoViewModel() {
                MyProfile = user,
                Video = await _archive.GetVideo(id)
            };

            return View(model);
        }
    }
}
