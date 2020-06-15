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
using PinkUmbrella.ViewModels.Post;
using PinkUmbrella.ViewModels.Home;

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    public class PostController : BaseController
    {
        private readonly ILogger<PostController> _logger;

        public PostController(IWebHostEnvironment environment, ILogger<PostController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IReactionService reactions):
            base(environment, signInManager, userManager, posts, userProfiles, reactions)
        {
            _logger = logger;
        }

        [Route("/Post/{id}")]
        public async Task<IActionResult> Index(int? id)
        {
            ViewData["Controller"] = "Post";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            if (id != null)
            {
                return View(new PostViewModel() {
                    Post = await _posts.GetPost(id.Value, user?.Id ?? -1),
                    MyProfile = user
                });
            }
            else
            {
                return View();
            }
        }

        [Authorize]
        public async Task<IActionResult> Unblock(int id)
        {
            var user = await GetCurrentUserAsync();
            await _reactions.UnReact(user.Id, id, ReactionType.Block, ReactionSubject.Post);
            return await ViewPost(id);
        }

        [Authorize]
        public async Task<IActionResult> Block(int id)
        {
            var user = await GetCurrentUserAsync();
            await _reactions.React(user.Id, id, ReactionType.Block, ReactionSubject.Post);
            return await ViewPost(id);
        }

        [Authorize]
        public async Task<IActionResult> Report(int id)
        {
            var user = await GetCurrentUserAsync();
            await _reactions.React(user.Id, id, ReactionType.Report, ReactionSubject.Post);
            return await ViewPost(id);
        }

        public async Task<IActionResult> ViewPost(int id)
        {
            var user = await GetCurrentUserAsync();
            var post = await _posts.GetPost(id, user?.Id);

            ViewData["PartialName"] = "Post/_Container";
            return View("_NoLayout", post);
        }

        [HttpPost]
        public async Task<IActionResult> NewPost(NewPostViewModel model)
        {
            var user = await GetCurrentUserAsync();
            var result = await _posts.TryCreateTextPosts(user.Id, model.Content, model.Visibility);
            if (!result.Error)
            {
                return RedirectToAction(nameof(Index), new { Id = result.Posts.First().Id });
            }
            else
            {
                ViewData["errorMsg"] = result.Message;
                return View();
            }
        }
    }
}
