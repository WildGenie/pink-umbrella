using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Person;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Services.Local;
using Tides.Models.Public;
using Estuary.Core;
using Estuary.Services;
using Estuary.Util;
using PinkUmbrella.ViewModels.Shared;
using static Estuary.Actors.Common;
using static Estuary.Objects.Common;
using System;
using PinkUmbrella.ViewModels;
using System.Collections.Generic;
using static Estuary.Activities.Common;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerPerson)]
    public partial class PersonController: ActivityStreamController
    {
        private readonly ILogger<PersonController> _logger;

        // asp-route-id="@Model.Profile.PublicId"
        private static readonly NavigationViewModel ProfileNav = new NavigationViewModel
        {
            Nodes = new NavigationViewModel[]
            {
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Index", Text = "Posts", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                    Nodes = new NavigationViewModel[]
                    {
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Notes", Text = "Notes", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Articles", Text = "Articles", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Photos", Text = "Photos", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Videos", Text = "Videos", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        }
                    }
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Shares", Text = "Shares", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Replies", Text = "Replies", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Mentions", Text = "Mentions", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Separator,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Shops", Text = "Shops", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Separator,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Followers", Text = "Followers", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Following", Text = "Following", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Separator,
                },
                new NavigationViewModel
                {
                    Type = NavigationViewModel.NavType.Link,
                    Action = "Activity", Text = "Activity", Controller = "Person",
                    GetRouteData = GetNavRouteData,
                    Nodes = new NavigationViewModel[]
                    {
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Likes", Text = "Likes", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Dislikes", Text = "Dislikes", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Upvotes", Text = "Upvotes", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Downvotes", Text = "Downvotes", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Ignored", Text = "Ignored", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Blocked", Text = "Blocked", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        },
                        new NavigationViewModel
                        {
                            Type = NavigationViewModel.NavType.Link,
                            Action = "Reported", Text = "Reported", Controller = "Person",
                            GetRouteData = GetNavRouteData,
                        }
                    }
                },
            }
        };

        private static Dictionary<string, string> GetNavRouteData(BaseViewModel model)
        {
            var pubId = (model as PersonViewModel)?.Profile?.PublicId;
            return new Dictionary<string, string>
            {
                { "id", pubId.IsLocal ? pubId.Id.ToString() : $"{pubId.PeerId}-{pubId.Id}" }
            };
        }

        public PersonController(
            IWebHostEnvironment environment,
            ILogger<PersonController> logger,
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IPostService posts,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            IReactionService reactions,
            ITagService tags,
            INotificationService notifications,
            IPeerService peers,
            IAuthService auth,
            ISettingsService settings,
            IActivityStreamRepository activityStreams):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
            _logger = logger;
        }

        [Route("/Person")]
        public async Task<IActionResult> Index() => RedirectToAction(nameof(Index), new { id = (await GetCurrentUserAsync()).UserId });

        [Route("/Person/{id}"), AllowAnonymous]
        public Task<IActionResult> Index(string id) => ViewBox(id, null);

        [Route("/Person/{id}/Shares"), AllowAnonymous]
        public Task<IActionResult> Shares(string id) => ViewBox(id, nameof(Shares));

        [Route("/Person/{id}/Followers"), AllowAnonymous]
        public Task<IActionResult> Followers(string id) => ViewBox(id, nameof(Followers));

        [Route("/Person/{id}/Following"), AllowAnonymous]
        public Task<IActionResult> Following(string id) => ViewBox(id, nameof(Following));

        [Route("/Person/{id}/Replies"), AllowAnonymous]
        public Task<IActionResult> Replies(string id) => ViewBox(id, nameof(Replies));

        [Route("/Person/{id}/Mentions"), AllowAnonymous]
        public Task<IActionResult> Mentions(string id) => ViewBox(id, nameof(Mentions));

        [Route("/Person/{id}/Shops"), AllowAnonymous]
        public Task<IActionResult> Shops(string id) => ViewBox(id, nameof(Shops));

        [Route("/Person/{id}/Notes"), AllowAnonymous]
        public Task<IActionResult> Notes(string id) => ViewBox(id, nameof(Notes));

        [Route("/Person/{id}/Articles"), AllowAnonymous]
        public Task<IActionResult> Articles(string id) => ViewBox(id, nameof(Articles));

        [Route("/Person/{id}/Photos"), AllowAnonymous]
        public Task<IActionResult> Photos(string id) => ViewBox(id, nameof(Photos));

        [Route("/Person/{id}/Videos"), AllowAnonymous]
        public Task<IActionResult> Videos(string id) => ViewBox(id, nameof(Videos));

        [Route("/Person/{id}/ArchivedMedia"), AllowAnonymous]
        public Task<IActionResult> ArchivedMedia(string id) => ViewBox(id, nameof(ArchivedMedia));

        [Route("/Person/{id}/Activity"), AllowAnonymous]
        public Task<IActionResult> Activity(string id) => ViewBox(id, nameof(Activity));

        [Route("/Person/{id}/Likes"), AllowAnonymous]
        public Task<IActionResult> Likes(string id) => ViewBox(id, nameof(Likes));

        [Route("/Person/{id}/Dislikes"), AllowAnonymous]
        public Task<IActionResult> Dislikes(string id) => ViewBox(id, nameof(Dislikes));

        [Route("/Person/{id}/Upvotes"), AllowAnonymous]
        public Task<IActionResult> Upvotes(string id) => ViewBox(id, nameof(Upvotes));

        [Route("/Person/{id}/Downvotes"), AllowAnonymous]
        public Task<IActionResult> Downvotes(string id) => ViewBox(id, nameof(Downvotes));

        [Route("/Person/{id}/Ignored"), AllowAnonymous]
        public Task<IActionResult> Ignored(string id) => ViewBox(id, nameof(Ignored));

        [Route("/Person/{id}/Blocked"), AllowAnonymous]
        public Task<IActionResult> Blocked(string id) => ViewBox(id, nameof(Blocked));

        [Route("/Person/{id}/Reported"), AllowAnonymous]
        public Task<IActionResult> Reported(string id) => ViewBox(id, nameof(Reported));

        [Route("/Person/{id}/{box}"), AllowAnonymous]
        public Task<IActionResult> ViewBox(string id, string box)
        {
            id = Uri.UnescapeDataString(id);
            ViewData["Controller"] = "Person";
            var pid = new PublicId(id);
            pid.Type = "Person";
            var filter = new ActivityStreamFilter("outbox")
            {
                id = pid,
            };
            string emptyMessage = null;

            if (string.IsNullOrWhiteSpace(box))
            {
                ViewData["Action"] = nameof(Index);
                filter = new ActivityStreamFilter("outbox")
                {
                    id = pid, includeReplies = false
                }.FixType(nameof(Create));
                emptyMessage = "This user has not posted anything.";
            }
            else
            {
                ViewData["Action"] = box;
                switch (box.Trim().ToLower())
                {
                    case "shares":
                    ViewData["Action"] = "Shares";
                    emptyMessage = "This user has not shared anything.";
                    filter.FixType(nameof(Announce));
                    break;
                    case "replies":
                    ViewData["Action"] = "Replies";
                    emptyMessage = "This user has not replied to anything.";
                    filter.includeReplies = true;
                    break;
                    case "notes":
                    ViewData["Action"] = nameof(Notes);
                    emptyMessage = "This user has not made any notes.";
                    filter.FixType(nameof(Create)).FixObjType(nameof(Note));
                    break;
                    case "articles":
                    ViewData["Action"] = nameof(Articles);
                    emptyMessage = "This user has not written any articles.";
                    filter.FixType(nameof(Create)).FixObjType(nameof(Article));
                    break;
                    case "photos":
                    ViewData["Action"] = nameof(Photos);
                    emptyMessage = "This user has not uploaded any photos.";
                    filter.FixType(nameof(Create)).FixObjType(nameof(Image));
                    break;
                    case "videos":
                    ViewData["Action"] = nameof(Videos);
                    emptyMessage = "This user has not uploaded any videos.";
                    filter.FixType(nameof(Create)).FixObjType(nameof(Video));
                    break;
                    case "archivedmedia":
                    ViewData["Action"] = nameof(ArchivedMedia);
                    emptyMessage = "This user has not uploaded any media.";
                    filter.FixType(nameof(Create)).FixObjType(nameof(Document));
                    break;
                    // case "mentions":
                    // ViewData["Action"] = nameof(Mentions);
                    // emptyMessage = "This user has not been mentioned.";
                    // filter = new ActivityStreamFilter("outbox")
                    // {
                    //     id = pid,
                    // }.FixType(nameof(Mention));
                    // break;
                    case "shops":
                    ViewData["Action"] = nameof(Shops);
                    emptyMessage = "This user has no shops.";
                    filter.FixObjType(nameof(Organization));
                    break;
                    case "following":
                    ViewData["Action"] = nameof(Following);
                    emptyMessage = "This user is not following anyone.";
                    filter = new ActivityStreamFilter("following")
                    {
                        id = pid,
                    }.FixObjType(nameof(Person));
                    break;
                    case "followers":
                    ViewData["Action"] = nameof(Followers);
                    emptyMessage = "This user has no followers.";
                    filter = new ActivityStreamFilter("followers")
                    {
                        id = pid,
                    }.FixObjType(nameof(Person));
                    break;
                    case "likes":
                    ViewData["Action"] = nameof(Likes);
                    emptyMessage = "This user has no likes.";
                    filter.FixType(nameof(Like));
                    break;
                    case "dislikes":
                    ViewData["Action"] = nameof(Dislikes);
                    emptyMessage = "This user has no dislikes.";
                    filter.FixType(nameof(Dislike));
                    break;
                    case "upvotes":
                    ViewData["Action"] = nameof(Upvotes);
                    emptyMessage = "This user has no upvotes.";
                    filter.FixType(nameof(Upvote));
                    break;
                    case "downvotes":
                    ViewData["Action"] = nameof(Downvotes);
                    emptyMessage = "This user has no downvotes.";
                    filter.FixType(nameof(Downvote));
                    break;
                    case "ignored":
                    ViewData["Action"] = nameof(Ignored);
                    emptyMessage = "This user has not ignored anything or anyone.";
                    filter.FixType(nameof(Ignore));
                    break;
                    case "blocked":
                    ViewData["Action"] = nameof(Blocked);
                    emptyMessage = "This user has not blocked anything or anyone.";
                    filter.FixType(nameof(Block));
                    break;
                    case "reported":
                    ViewData["Action"] = nameof(Reported);
                    emptyMessage = "This user has not reported anything or anyone.";
                    filter.FixType(nameof(Report));
                    break;
                }
            }
            return ViewCollection(filter, emptyMessage);
        }

        private async Task<IActionResult> ViewCollection(ActivityStreamFilter filter, string emptyMessage)
        {
            var currentUser = await GetCurrentUserAsync();
            filter.viewerId = currentUser.PublicId.IsLocal ? currentUser.UserId : null;
            var user = await _publicProfiles.GetUser(filter.id, currentUser?.UserId);
            if (user != null)
            {
                var items = await _activityStreams.GetAll(filter);
                if (IsActivityStreamRequest)
                {
                    return ActivityStream(items);
                }
                else
                {
                    var lvm = ListViewModel.Regular(items);
                    lvm.EmptyViewModel = emptyMessage;
                    return View(nameof(Index), new IndexViewModel()
                    {
                        MyProfile = currentUser,
                        Profile = user,
                        Items = lvm,
                        ItemNav = ProfileNav,
                    });
                }
            }
            return NotFound();
        }

        [Route("/Person/Completions/{prefix}")]
        public async Task<IActionResult> Completions(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var user = await GetCurrentUserAsync();
                var tags = await _localProfiles.GetCompletionsFor(prefix, user.UserId.Value);
                return Json(new {
                    items = tags.Select(t => new { value = t.Id.ToString(), label = t.Handle, display = t.DisplayName }).ToArray()
                });
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> PersonByHandle(string handle)
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _localProfiles.GetUser(handle, currentUser?.UserId);
            if (user == null)
            {
                return Redirect("/Error/404");
            }
            else if (IsActivityStreamRequest)
            {
                return ActivityStream(await _activityStreams.Get(new ActivityStreamFilter(null) { handle = handle }.FixObjType("Person")));
            }
            else
            {
                return RedirectToAction(nameof(Index), new { Id = user.Id });
            }
        }

        // public async Task<IActionResult> ViewPerson(int id)
        // {
        //     var cuser = await GetCurrentUserAsync();
        //     var user = await _localProfiles.GetUser(id, cuser?.Id);

        //     ViewData["PartialName"] = "Person/_Container";
        //     return View("_NoLayout", new PersonViewModel() {
        //         MyProfile = cuser,
        //         Profile = user,
        //     });
        // }
    }
}