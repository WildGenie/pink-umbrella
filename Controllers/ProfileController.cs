using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Profile;

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    public class ProfileController: BaseController
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IArchiveService _archive;

        public ProfileController(IWebHostEnvironment environment, ILogger<ProfileController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IReactionService reactions,
            IArchiveService archive):
            base(environment, signInManager, userManager, posts, userProfiles, reactions)
        {
            _logger = logger;
            _archive = archive;
        }

        [Route("/Profile"), Authorize]
        public async Task<IActionResult> Index() => RedirectToAction(nameof(Index), new { id = (await GetCurrentUserAsync()).Id });

        [Route("/Profile/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Index);
            var currentUser = await GetCurrentUserAsync();
            var user = await _userProfiles.GetUser(id, currentUser?.Id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return View(new IndexViewModel() {
                    MyProfile = currentUser,
                    Profile = user,
                    Feed = await _posts.GetPostsForUser(user.Id, currentUser?.Id, false, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }
        
        [Route("/Profile/{id}/Replies")]
        public async Task<IActionResult> Replies(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Replies);
            var currentUser = await GetCurrentUserAsync();
            var user = await _userProfiles.GetUser(id, currentUser?.Id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return View("Index", new IndexViewModel() {
                    MyProfile = currentUser,
                    Profile = user,
                    Feed = await _posts.GetPostsForUser(user.Id, currentUser?.Id, true, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }
        
        [Route("/Profile/{id}/Mentions")]
        public async Task<IActionResult> Mentions(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Mentions);
            var currentUser = await GetCurrentUserAsync();
            var user = await _userProfiles.GetUser(id, currentUser?.Id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return View("Index", new IndexViewModel() {
                    MyProfile = currentUser,
                    Profile = user,
                    Feed = await _posts.GetMentionsForUser(user.Id, currentUser?.Id, false, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }
        
        [Route("/Profile/{id}/Photos")]
        public async Task<IActionResult> Photos(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Photos);
            return await ViewMedia(id, ArchivedMediaType.Photo);
        }
        
        [Route("/Profile/{id}/Videos")]
        public async Task<IActionResult> Videos(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Videos);
            return await ViewMedia(id, ArchivedMediaType.Video);
        }
        
        [Route("/Profile/{id}/ArchivedMedia")]
        public async Task<IActionResult> ArchivedMedia(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(ArchivedMedia);
            return await ViewMedia(id, null);
        }

        private async Task<IActionResult> ViewMedia(int id, ArchivedMediaType? type)
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userProfiles.GetUser(id, currentUser?.Id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return View("Index", new IndexViewModel()
                {
                    MyProfile = currentUser,
                    Profile = user,
                    Media = await _archive.GetMediaForUser(user.Id, currentUser?.Id, type, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }

        public async Task<IActionResult> ProfileByHandle(string handle)
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userProfiles.GetUser(handle, currentUser?.Id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return RedirectToAction(nameof(Index), new { Id = user.Id });
            }
        }

        // public async Task<IActionResult> ViewProfile(int id)
        // {
        //     var cuser = await GetCurrentUserAsync();
        //     var user = await _userProfiles.GetUser(id, cuser?.Id);

        //     ViewData["PartialName"] = "Profile/_Container";
        //     return View("_NoLayout", new ProfileViewModel() {
        //         MyProfile = cuser,
        //         Profile = user,
        //     });
        // }
    }
}