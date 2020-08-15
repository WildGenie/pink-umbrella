using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Post;
using PinkUmbrella.ViewModels.Home;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Services.Local;
using Tides.Models;
using Tides.Models.Public;
using Estuary.Core;
using Estuary.Services;
using Estuary.Objects;
using Estuary.Util;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerPost)]
    [AllowAnonymous]
    public class PostController : ActivityStreamController
    {
        private readonly ILogger<PostController> _logger;

        public PostController(
            IWebHostEnvironment environment,
            ILogger<PostController> logger,
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

        [Route("/Post/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            ViewData["Controller"] = "Post";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            if (id != null)
            {
                var post = await _activityStreams.Get(new ActivityStreamFilter("outbox")
                {
                    id = new PublicId(id), viewerId = user?.UserId,
                }.FixObjType("Note", "Article"));
                if (post != null)
                {
                    return View(new PostViewModel() {
                        Post = post,
                        MyProfile = user
                    });
                }
                else
                {
                    return Redirect("/Error/404");
                }
            }
            else
            {
                return View();
            }
        }

        [Authorize]
        public async Task<IActionResult> Unblock(string id)
        {
            var pid = new PublicId(id);
            var user = await GetCurrentUserAsync();
            await _reactions.UnReact(user.UserId.Value, pid, ReactionType.Block);
            return await ViewPost(pid);
        }

        [Authorize]
        public async Task<IActionResult> Block(string id)
        {
            var pid = new PublicId(id);
            var user = await GetCurrentUserAsync();
            await _reactions.React(user.UserId.Value, pid, ReactionType.Block);
            return await ViewPost(pid);
        }

        [Authorize]
        public async Task<IActionResult> Report(string id)
        {
            var pid = new PublicId(id);
            var user = await GetCurrentUserAsync();
            await _reactions.React(user.UserId.Value, pid, ReactionType.Report);
            return await ViewPost(pid);
        }

        public Task<IActionResult> ViewPost(string id) => ViewPost(new PublicId(id));

        private async Task<IActionResult> ViewPost(PublicId id)
        {
            var user = await GetCurrentUserAsync();
            var post = await _activityStreams.Get(new ActivityStreamFilter("outbox") { id = id, viewerId = user?.UserId });

            ViewData["PartialName"] = "Activity/_Container";
            return View("_NoLayout", post);
        }

        [HttpPost]
        public async Task<IActionResult> NewPost(NewPostViewModel model)
        {
            var user = await GetCurrentUserAsync();
            var result = await _posts.TryCreateTextPost(user, model.Content, model.Visibility);
            if (result is Error err)
            {
                ViewData["errorMsg"] = $"{err.errorCode}: {err.content}";
                return View();
            }
            else if (result is ActivityObject action)
            {
                return RedirectToAction(nameof(PersonController.Index), user.type, new { id = user.PublicId, highlightActivity = action.id });
            }
            else
            {
                return BadRequest($"Error: {result.type}");
            }
        }
    }
}
