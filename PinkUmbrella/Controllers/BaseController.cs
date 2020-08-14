using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using Microsoft.FeatureManagement;
using PinkUmbrella.ViewModels.Shared;
using PinkUmbrella.Services.Local;
using Estuary.Actors;

namespace PinkUmbrella.Controllers
{
    public class BaseController: Controller
    {
        protected readonly SignInManager<UserProfileModel> _signInManager;
        protected readonly UserManager<UserProfileModel> _userManager;
        protected readonly IUserProfileService _localProfiles;
        protected readonly IPublicProfileService _publicProfiles;
        protected readonly IAuthService _auth;
        protected readonly IFeatureManager _featureManager;
        protected readonly ISettingsService _settings;
        
        public BaseController(
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles, 
            IAuthService auth,
            ISettingsService settings
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _localProfiles = localProfiles;
            _publicProfiles = publicProfiles;
            _auth = auth;
            _settings = settings;
            _featureManager = settings.FeatureManager;
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
    }
}