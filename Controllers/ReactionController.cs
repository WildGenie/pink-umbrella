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
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Shared;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerReaction)]
    [Authorize]
    public class ReactionController: BaseController
    {
        private readonly ILogger<ReactionController> _logger;

        public ReactionController(IWebHostEnvironment environment, ILogger<ReactionController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles,IReactionService reactions, ITagService tags,
            INotificationService notifications, IPeerService peers, IAuthService auth, ISettingsService settings):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications, peers, auth, settings)
        {
            _logger = logger;
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

            ViewData["PartialName"] = "Button/ReactButton";
            return View("_NoLayout", new ReactViewModel()
            {
                HasReacted = true,
                Subject = subject,
                ToId = toId,
                Type = t,
                ViewerId = user.Id,
                Count = await _reactions.GetCount(toId, t, subject),
            });
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

            ViewData["PartialName"] = "Button/ReactButton";
            return View("_NoLayout", new ReactViewModel()
            {
                HasReacted = false,
                Subject = subject,
                ToId = toId,
                Type = t,
                ViewerId = user.Id,
                Count = await _reactions.GetCount(toId, t, subject),
            });
        }
    }
}