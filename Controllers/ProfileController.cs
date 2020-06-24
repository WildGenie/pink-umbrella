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
        private readonly IShopService _shops;

        public ProfileController(IWebHostEnvironment environment, ILogger<ProfileController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IReactionService reactions,
            IArchiveService archive, ITagService tags, IShopService shops, INotificationService notifications):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications)
        {
            _logger = logger;
            _archive = archive;
            _shops = shops;
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
        
        [Route("/Profile/{id}/Shops")]
        public async Task<IActionResult> Shops(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Shops);
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
                    Shops = await _shops.GetShopsForUser(user.Id, currentUser?.Id) // , new PaginationModel() { count = 10, start = 0 }
                });
            }
        }
        
        [Route("/Profile/{id}/Following")]
        public async Task<IActionResult> Following(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Following);
            return await ViewUserList(id, UserListType.Following);
        }

        [Route("/Profile/{id}/Followers")]
        public async Task<IActionResult> Followers(int id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Followers);
            return await ViewUserList(id, UserListType.Followers);
        }

        private async Task<IActionResult> ViewUserList(int id, UserListType type)
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userProfiles.GetUser(id, currentUser?.Id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                var list = new List<UserProfileModel>();
                switch (type)
                {
                    case UserListType.Followers:
                    list = await _userProfiles.GetFollowers(user.Id, currentUser?.Id);
                    break;
                    case UserListType.Following:
                    list = await _userProfiles.GetFollowing(user.Id, currentUser?.Id);
                    break;
                }
                return ViewUserListForUser(user, currentUser, type, list);
            }
        }

        private IActionResult ViewUserListForUser(UserProfileModel user, UserProfileModel currentUser, UserListType type, List<UserProfileModel> list)
        {
            return View("Index", new IndexViewModel()
            {
                MyProfile = currentUser,
                Profile = user,
                Users = list,
                UserListType = type,
            });
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