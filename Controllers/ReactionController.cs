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
using PinkUmbrella.Models.Public;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Local;
using PinkUmbrella.ViewModels.Shared;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerReaction)]
    [Authorize]
    public class ReactionController: BaseController
    {
        private readonly ILogger<ReactionController> _logger;

        public ReactionController(IWebHostEnvironment environment, ILogger<ReactionController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService localProfiles, IPublicProfileService publicProfiles,IReactionService reactions, ITagService tags,
            INotificationService notifications, IPeerService peers, IAuthService auth, ISettingsService settings):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Like(string id, ReactionSubject subject) => await React(new PublicId(id), ReactionType.Like, subject);

        public async Task<IActionResult> Dislike(string id, ReactionSubject subject) => await React(new PublicId(id), ReactionType.Dislike, subject);

        public async Task<IActionResult> Report(string id, ReactionSubject subject) => await React(new PublicId(id), ReactionType.Report, subject);

        public async Task<IActionResult> Block(string id, ReactionSubject subject) => await React(new PublicId(id), ReactionType.Block, subject);

        public async Task<IActionResult> Follow(string id, ReactionSubject subject) => await React(new PublicId(id), ReactionType.Follow, subject);

        private async Task<IActionResult> React(PublicId toId, ReactionType t, ReactionSubject subject)
        {
            var user = await GetCurrentUserAsync();
            var reactionId = await _reactions.React(user.UserId, toId, t, subject);

            ViewData["PartialName"] = "Button/ReactButton";
            return View("_NoLayout", new ReactViewModel()
            {
                HasReacted = true,
                Subject = subject,
                ToId = toId,
                Type = t,
                ViewerId = user.UserId,
                Count = await _reactions.GetCount(toId, t, subject),
            });
        }

        public async Task<IActionResult> UnLike(string id, ReactionSubject subject) => await UnReact(new PublicId(id), ReactionType.Like, subject);

        public async Task<IActionResult> UnDislike(string id, ReactionSubject subject) => await UnReact(new PublicId(id), ReactionType.Dislike, subject);

        public async Task<IActionResult> UnReport(string id, ReactionSubject subject) => await UnReact(new PublicId(id), ReactionType.Report, subject);

        public async Task<IActionResult> UnBlock(string id, ReactionSubject subject) => await UnReact(new PublicId(id), ReactionType.Block, subject);

        public async Task<IActionResult> UnFollow(string id, ReactionSubject subject) => await UnReact(new PublicId(id), ReactionType.Follow, subject);

        private async Task<IActionResult> UnReact(PublicId toId, ReactionType t, ReactionSubject subject)
        {
            var user = await GetCurrentUserAsync();
            await _reactions.UnReact(user.UserId, toId, t, subject);

            ViewData["PartialName"] = "Button/ReactButton";
            return View("_NoLayout", new ReactViewModel()
            {
                HasReacted = false,
                Subject = subject,
                ToId = toId,
                Type = t,
                ViewerId = user.UserId,
                Count = await _reactions.GetCount(toId, t, subject),
            });
        }
    }
}