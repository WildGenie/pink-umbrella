using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerNotification)]
    public class NotificationController : BaseController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;

        public NotificationController(IWebHostEnvironment environment, ILogger<ShopController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IShopService shops,
            IReactionService reactions, ITagService tags, INotificationService notifications, IPeerService peers, IAuthService auth,
            ISettingsService settings):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications, peers, auth, settings)
        {
            _logger = logger;
            _shops = shops;
        }

        public async Task<IActionResult> ViewedSince(int id)
        {
            var user = await GetCurrentUserAsync();
            await _notifications.ViewedSince(user.Id, id);
            return Ok();
        }

        public async Task<IActionResult> DismissSince(int id)
        {
            var user = await GetCurrentUserAsync();
            await _notifications.DismissSince(user.Id, id);
            return Ok();
        }

        public async Task<IActionResult> Dismissed(int id)
        {
            var user = await GetCurrentUserAsync();
            await _notifications.Dismiss(user.Id, id);
            return Ok();
        }
    }
}
