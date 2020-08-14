using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Services.Local;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerNotification)]
    public class NotificationController : BaseController
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationService _notifications;

        public NotificationController(
            ILogger<NotificationController> logger,
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            INotificationService notifications,
            IAuthService auth,
            ISettingsService settings):
            base(signInManager, userManager, localProfiles, publicProfiles, auth, settings)
        {
            _logger = logger;
            _notifications = notifications;
        }

        public async Task<IActionResult> ViewedSince(int id)
        {
            var user = await GetCurrentLocalUserAsync();
            await _notifications.ViewedSince(user.Id, id);
            return Ok();
        }

        public async Task<IActionResult> DismissSince(int id)
        {
            var user = await GetCurrentLocalUserAsync();
            await _notifications.DismissSince(user.Id, id);
            return Ok();
        }

        public async Task<IActionResult> Dismissed(int id)
        {
            var user = await GetCurrentLocalUserAsync();
            await _notifications.Dismiss(user.Id, id);
            return Ok();
        }
    }
}
