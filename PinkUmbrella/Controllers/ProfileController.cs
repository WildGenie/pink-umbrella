using System.Collections.Generic;
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
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Community;
using PinkUmbrella.Services.Local;
using PinkUmbrella.Models.Public;
using Poncho.Models.Public;
using Poncho.Models;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerProfile)]
    public partial class ProfileController: BaseController
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IArchiveService _archive;
        private readonly IShopService _shops;

        public ProfileController(IWebHostEnvironment environment, ILogger<ProfileController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService localProfiles, IPublicProfileService publicProfiles, IReactionService reactions,
            IArchiveService archive, ITagService tags, IShopService shops, INotificationService notifications, IPeerService peers, IAuthService auth,
            ISettingsService settings):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings)
        {
            _logger = logger;
            _archive = archive;
            _shops = shops;
        }

        [Route("/Profile")]
        public async Task<IActionResult> Index() => RedirectToAction(nameof(Index), new { id = (await GetCurrentUserAsync()).Id });

        [Route("/Profile/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Index);
            var currentUser = await GetCurrentUserAsync();
            var user = await _publicProfiles.GetUser(new PublicId(id), currentUser?.UserId);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return View(new IndexViewModel() {
                    MyProfile = currentUser,
                    Profile = user,
                    Feed = await _posts.GetPostsForUser(user.PublicId, currentUser?.UserId, false, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }
        
        [Route("/Profile/{id}/Replies")]
        [AllowAnonymous]
        public async Task<IActionResult> Replies(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Replies);
            var currentUser = await GetCurrentUserAsync();
            var user = await _publicProfiles.GetUser(new PublicId(id), currentUser?.UserId);
            if (user == null)
            {
                return Redirect("/Error/404");
            }
            else
            {
                return View("Index", new IndexViewModel() {
                    MyProfile = currentUser,
                    Profile = user,
                    Feed = await _posts.GetPostsForUser(user.PublicId, currentUser?.UserId, true, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }
        
        [Route("/Profile/{id}/Mentions")]
        [AllowAnonymous]
        public async Task<IActionResult> Mentions(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Mentions);
            var currentUser = await GetCurrentUserAsync();
            var user = await _publicProfiles.GetUser(new PublicId(id), currentUser?.UserId);
            if (user == null)
            {
                return Redirect("/Error/404");
            }
            else
            {
                return View("Index", new IndexViewModel() {
                    MyProfile = currentUser,
                    Profile = user,
                    Feed = await _posts.GetMentionsForUser(user.PublicId, currentUser?.UserId, false, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }
        
        [Route("/Profile/{id}/Photos")]
        [AllowAnonymous]
        public async Task<IActionResult> Photos(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Photos);
            return await ViewMedia(id, ArchivedMediaType.Photo);
        }
        
        [Route("/Profile/{id}/Videos")]
        [AllowAnonymous]
        public async Task<IActionResult> Videos(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Videos);
            return await ViewMedia(id, ArchivedMediaType.Video);
        }
        
        [Route("/Profile/{id}/ArchivedMedia")]
        [AllowAnonymous]
        public async Task<IActionResult> ArchivedMedia(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(ArchivedMedia);
            return await ViewMedia(id, null);
        }
        
        [Route("/Profile/{id}/Shops")]
        [AllowAnonymous]
        public async Task<IActionResult> Shops(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Shops);
            var currentUser = await GetCurrentUserAsync();
            var user = await _publicProfiles.GetUser(new PublicId(id), currentUser?.UserId);
            if (user == null)
            {
                return Redirect("/Error/404");
            }
            else
            {
                return View("Index", new IndexViewModel()
                {
                    MyProfile = currentUser,
                    Profile = user,
                    Shops = await _shops.GetShopsForUser(user.PublicId, currentUser?.UserId) // , new PaginationModel() { count = 10, start = 0 }
                });
            }
        }
        
        [Route("/Profile/{id}/Following")]
        [AllowAnonymous]
        public async Task<IActionResult> Following(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Following);
            return await ViewUserList(id, UserListType.Following);
        }

        [Route("/Profile/{id}/Followers")]
        [AllowAnonymous]
        public async Task<IActionResult> Followers(string id)
        {
            ViewData["Controller"] = "Profile";
            ViewData["Action"] = nameof(Followers);
            return await ViewUserList(id, UserListType.Followers);
        }

        private async Task<IActionResult> ViewUserList(string id, UserListType type)
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _publicProfiles.GetUser(new PublicId(id), currentUser?.UserId);
            if (user == null)
            {
                return Redirect("/Error/404");
            }
            else
            {
                PublicProfileModel[] list = null;
                switch (type)
                {
                    case UserListType.Followers:
                    list = await _publicProfiles.GetFollowers(user.PublicId, currentUser?.UserId);
                    break;
                    case UserListType.Following:
                    list = await _publicProfiles.GetFollowing(user.PublicId, currentUser?.UserId);
                    break;
                }
                return ViewUserListForUser(user, currentUser, type, list);
            }
        }

        private IActionResult ViewUserListForUser(PublicProfileModel user, PublicProfileModel currentUser, UserListType type, PublicProfileModel[] list)
        {
            return View("Index", new IndexViewModel()
            {
                MyProfile = currentUser,
                Profile = user,
                Users = list,
                UserListType = type,
            });
        }

        private async Task<IActionResult> ViewMedia(string id, ArchivedMediaType? type)
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _publicProfiles.GetUser(new PublicId(id), currentUser?.UserId);
            if (user == null)
            {
                return Redirect("/Error/404");
            }
            else
            {
                return View("Index", new IndexViewModel()
                {
                    MyProfile = currentUser,
                    Profile = user,
                    Media = await _archive.GetMediaForUser(user.PublicId, currentUser?.UserId, type, new PaginationModel() { count = 10, start = 0 })
                });
            }
        }

        public async Task<IActionResult> ProfileByHandle(string handle)
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _localProfiles.GetUser(handle, currentUser?.UserId);
            if (user == null)
            {
                return Redirect("/Error/404");
            }
            else
            {
                return RedirectToAction(nameof(Index), new { Id = user.Id });
            }
        }

        [Route("/Profile/Completions/{prefix}")]
        public async Task<IActionResult> Completions(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var user = await GetCurrentUserAsync();
                var tags = await _localProfiles.GetCompletionsFor(prefix, user.UserId);
                return Json(new {
                    items = tags.Select(t => new { value = t.Id.ToString(), label = t.Handle, display = t.DisplayName }).ToArray()
                });
            }
            else
            {
                return NotFound();
            }
        }

        // public async Task<IActionResult> ViewProfile(int id)
        // {
        //     var cuser = await GetCurrentUserAsync();
        //     var user = await _localProfiles.GetUser(id, cuser?.Id);

        //     ViewData["PartialName"] = "Profile/_Container";
        //     return View("_NoLayout", new ProfileViewModel() {
        //         MyProfile = cuser,
        //         Profile = user,
        //     });
        // }
    }
}