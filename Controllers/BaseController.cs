using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using seattle.Models;
using seattle.Services;

namespace seattle.Controllers
{
    public class BaseController: Controller
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly SignInManager<UserProfileModel> _signInManager;
        protected readonly UserManager<UserProfileModel> _userManager;
        protected readonly IFeedService _feeds;
        protected readonly IUserProfileService _userProfiles;
        
        public BaseController(IWebHostEnvironment environment, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IFeedService feeds, IUserProfileService userProfiles)
        {
            _environment = environment;
            _signInManager = signInManager;
            _userManager = userManager;
            _feeds = feeds;
            _userProfiles = userProfiles;
        }

        protected Task<UserProfileModel> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}