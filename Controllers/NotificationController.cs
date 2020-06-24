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
using PinkUmbrella.ViewModels.Shop;

namespace PinkUmbrella.Controllers
{
    public class NotificationController : BaseController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;

        public NotificationController(IWebHostEnvironment environment, ILogger<ShopController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IShopService shops,
            IReactionService reactions, ITagService tags, INotificationService notifications):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications)
        {
            _logger = logger;
            _shops = shops;
        }

        public async Task<IActionResult> ViewedSince(int id)
        {
            ViewData["Controller"] = "Tag";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            await _notifications.ViewedSince(user.Id, id);
            return Ok();
        }

        public async Task<IActionResult> DismissSince(int id)
        {
            ViewData["Controller"] = "Tag";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            await _notifications.DismissSince(user.Id, id);
            return Ok();
        }

        public async Task<IActionResult> Dismissed(int id)
        {
            ViewData["Controller"] = "Tag";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            await _notifications.Dismiss(user.Id, id);
            return Ok();
        }
    }
}
