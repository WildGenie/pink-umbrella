using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using seattle.Models;
using seattle.Services;
using seattle.ViewModels.Home;
using seattle.ViewModels.Shared;

namespace seattle.Controllers
{
    [Authorize]
    public class ReactionController: BaseController
    {
        private readonly ILogger<ReactionController> _logger;
        private readonly IReactionService _reactions;

        public ReactionController(IWebHostEnvironment environment, ILogger<ReactionController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IReactionService reactions):
            base(environment, signInManager, userManager, posts, userProfiles)
        {
            _logger = logger;
            _reactions = reactions;
        }

        public async Task<IActionResult> Like(int id, ReactionSubject subject) => await React(id, ReactionType.Like, subject);

        public async Task<IActionResult> Dislike(int id, ReactionSubject subject) => await React(id, ReactionType.Dislike, subject);

        public async Task<IActionResult> Report(int id, ReactionSubject subject) => await React(id, ReactionType.Report, subject);

        public async Task<IActionResult> Block(int id, ReactionSubject subject) => await React(id, ReactionType.Block, subject);

        public async Task<IActionResult> Follow(int id, ReactionSubject subject) => await React(id, ReactionType.Follow, subject);

        private async Task<IActionResult> React(int toId, ReactionType t, ReactionSubject subject)
        {
            var user = await GetCurrentUserAsync();
            var reactionId = await _reactions.React(user.Id, toId, t, subject);

            // if (IsAjaxRequest())
            // {
            //     return Json(new
            //     {
            //         reactionId
            //     });
            // }
            // else
            // {
                ViewData["PartialName"] = "_ReactButton";
                return View("_NoLayout", new ReactViewModel()
                {
                    HasReacted = true,
                    Subject = subject,
                    ToId = toId,
                    Type = t,
                });
                // return View("StatusBar", new StatusViewModel()
                // {
                //     Message = GetMessage(t, subject),
                //     MSBeforeDestruction = 5000 // 5 seconds
                // });
            //}
        }

        public async Task<IActionResult> UnLike(int id, ReactionSubject subject) => await UnReact(id, ReactionType.Like, subject);

        public async Task<IActionResult> UnDislike(int id, ReactionSubject subject) => await UnReact(id, ReactionType.Dislike, subject);

        public async Task<IActionResult> UnReport(int id, ReactionSubject subject) => await UnReact(id, ReactionType.Report, subject);

        public async Task<IActionResult> UnBlock(int id, ReactionSubject subject) => await UnReact(id, ReactionType.Block, subject);

        public async Task<IActionResult> UnFollow(int id, ReactionSubject subject) => await UnReact(id, ReactionType.Follow, subject);

        private async Task<IActionResult> UnReact(int toId, ReactionType t, ReactionSubject subject)
        {
            var user = await GetCurrentUserAsync();
            await _reactions.UnReact(user.Id, toId, t, subject);

            // if (IsAjaxRequest())
            // {
            //     return Json(new
            //     {
            //         reactionId
            //     });
            // }
            // else
            // {
                ViewData["PartialName"] = "_ReactButton";
                return View("_NoLayout", new ReactViewModel()
                {
                    HasReacted = false,
                    Subject = subject,
                    ToId = toId,
                    Type = t,
                });
                // return View("StatusBar", new StatusViewModel()
                // {
                //     Message = GetUndoMessage(t, subject),
                //     MSBeforeDestruction = 5000 // 5 seconds
                // });
            //}
        }

        private string GetMessage(ReactionType t, ReactionSubject subject)
        {
            switch (subject)
            {
                case ReactionSubject.Post:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked post";
                    case ReactionType.Dislike:
                    return "Disliked post";
                    case ReactionType.Report:
                    return "Reported post";
                    case ReactionType.Block:
                    return "Blocked post";
                }
                break;
                case ReactionSubject.Profile:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked profile";
                    case ReactionType.Dislike:
                    return "Disliked profile";
                    case ReactionType.Report:
                    return "Reported profile";
                    case ReactionType.Block:
                    return "Blocked profile";
                    case ReactionType.Follow:
                    return "Followed profile";
                }
                break;
                case ReactionSubject.Shop:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked shop";
                    case ReactionType.Dislike:
                    return "Disliked shop";
                    case ReactionType.Report:
                    return "Reported shop";
                    case ReactionType.Block:
                    return "Blocked shop";
                    case ReactionType.Follow:
                    return "Followed shop";
                }
                break;
                case ReactionSubject.ArchivedMedia:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked media";
                    case ReactionType.Dislike:
                    return "Disliked media";
                    case ReactionType.Report:
                    return "Reported media";
                    case ReactionType.Block:
                    return "Blocked media";
                }
                break;
            }
            return "no message";
        }

        private string GetUndoMessage(ReactionType t, ReactionSubject subject)
        {
            switch (subject)
            {
                case ReactionSubject.Post:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked post";
                    case ReactionType.Dislike:
                    return "Un-disliked post";
                    case ReactionType.Block:
                    return "Unblocked post";
                }
                break;
                case ReactionSubject.Profile:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked profile";
                    case ReactionType.Dislike:
                    return "Un-disliked profile";
                    case ReactionType.Block:
                    return "Unblocked profile";
                    case ReactionType.Follow:
                    return "Unfollowed profile";
                }
                break;
                case ReactionSubject.Shop:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked shop";
                    case ReactionType.Dislike:
                    return "Un-disliked shop";
                    case ReactionType.Block:
                    return "Unblocked shop";
                    case ReactionType.Follow:
                    return "Unfollowed shop";
                }
                break;
                case ReactionSubject.ArchivedMedia:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked media";
                    case ReactionType.Dislike:
                    return "Un-disliked media";
                    case ReactionType.Block:
                    return "Unblocked media";
                }
                break;
            }
            return "no undo message";
        }
    }
}