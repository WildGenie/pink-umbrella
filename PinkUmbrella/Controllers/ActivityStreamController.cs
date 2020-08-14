using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Local;
using Estuary.Core;
using PinkUmbrella.Util;
using Estuary.Services;

namespace PinkUmbrella.Controllers
{
    public class ActivityStreamController: BaseController
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly IPostService _posts;
        protected readonly IReactionService _reactions;
        protected readonly ITagService _tags;
        protected readonly INotificationService _notifications;
        protected readonly IPeerService _peers;
        protected readonly IActivityStreamRepository _activityStreams;
        
        public ActivityStreamController(
            IWebHostEnvironment environment,
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
            IActivityStreamRepository activityStreams
            ): base(signInManager, userManager, localProfiles, publicProfiles, auth, settings)
        {
            _environment = environment;
            _posts = posts;
            _reactions = reactions;
            _tags = tags;
            _notifications = notifications;
            _peers = peers;
            _activityStreams = activityStreams;
        }

        protected bool IsActivityStreamRequest => Request.Headers["Content-Type"] == "application/activity+json";

        protected ActivityStreamActionResult ActivityStream(BaseObject result) => new ActivityStreamActionResult(result);
    }
}