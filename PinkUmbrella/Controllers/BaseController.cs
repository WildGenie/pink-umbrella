using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using PinkUmbrella.Models;
using PinkUmbrella.Services;
using Microsoft.FeatureManagement;
using PinkUmbrella.ViewModels.Shared;
using PinkUmbrella.Services.Local;
using Tides.Services;
using Tides.Core;
using PinkUmbrella.Util;
using Tides.Actors;

namespace PinkUmbrella.Controllers
{
    public class BaseController: Controller
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly SignInManager<UserProfileModel> _signInManager;
        protected readonly UserManager<UserProfileModel> _userManager;
        protected readonly IPostService _posts;
        protected readonly IUserProfileService _localProfiles;
        protected readonly IPublicProfileService _publicProfiles;
        protected readonly IReactionService _reactions;
        protected readonly ITagService _tags;
        protected readonly INotificationService _notifications;
        protected readonly IPeerService _peers;
        protected readonly IAuthService _auth;
        protected readonly IFeatureManager _featureManager;
        protected readonly ISettingsService _settings;
        protected readonly IActivityStreamRepository _activityStreams;
        
        public BaseController(IWebHostEnvironment environment, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService localProfiles,
            IPublicProfileService publicProfiles, 
            IReactionService reactions, ITagService tags, INotificationService notifications,
            IPeerService peers, IAuthService auth, ISettingsService settings, IActivityStreamRepository activityStreams)
        {
            _environment = environment;
            _signInManager = signInManager;
            _userManager = userManager;
            _posts = posts;
            _localProfiles = localProfiles;
            _publicProfiles = publicProfiles;
            _reactions = reactions;
            _tags = tags;
            _notifications = notifications;
            _peers = peers;
            _auth = auth;
            _settings = settings;
            _featureManager = settings.FeatureManager;
            _activityStreams = activityStreams;
        }

        protected async Task<ActorObject> GetCurrentUserAsync()
        {
            var local = await _userManager.GetUserAsync(HttpContext.User);
            return await _publicProfiles.Transform(local, 0, local?.Id);
        }

        protected Task<UserProfileModel> GetCurrentLocalUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        protected bool IsAjaxRequest()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return true;
            }
            else
            {
                return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }
        }

        protected bool IsActivityStreamRequest => Request.Headers["Content-Type"] == "application/activity+json";

        protected void ShowStatus(string statusMessage, string statusType)
        {
            if (!string.IsNullOrWhiteSpace(statusMessage) && !string.IsNullOrWhiteSpace(statusType))
            {
                ViewData["StatusBar"] = new StatusViewModel()
                {
                    Message = statusMessage,
                    AlertType = statusType,
                };
            }
        }

        protected ActivityStreamActionResult ActivityStream(BaseObject result)
        {
            return new ActivityStreamActionResult(result);
        }
    }
}