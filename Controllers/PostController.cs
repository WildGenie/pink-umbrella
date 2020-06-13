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
using seattle.ViewModels.Post;

namespace seattle.Controllers
{
    public class PostController : BaseController
    {
        private readonly ILogger<PostController> _logger;

        public PostController(IWebHostEnvironment environment, ILogger<PostController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles):
            base(environment, signInManager, userManager, posts, userProfiles)
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
                    Post = await _posts.GetPost(id.Value),
                    MyProfile = user
                });
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewPost(string[] Content, Visibility visibility)
        {
            var user = await GetCurrentUserAsync();
            var result = await _posts.TryCreateTextPosts(user.Id, Content, visibility);
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
