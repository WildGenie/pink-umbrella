using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Services.Local;
using PinkUmbrella.ViewModels.Doc;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerDoc)]
    [AllowAnonymous]
    public class DocController : BaseController
    {
        public DocController(
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            IAuthService auth,
            ISettingsService settings):
            base(signInManager, userManager, localProfiles, publicProfiles, auth, settings)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Doc";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            var model = new BaseViewModel()
            {
                MyProfile = user,
            };
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Api()
        {
            ViewData["Controller"] = "Doc";
            ViewData["Action"] = nameof(Api);
            return View(new BaseViewModel() { MyProfile = await GetCurrentUserAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> Data(string selected)
        {
            ViewData["Controller"] = "Doc";
            ViewData["Action"] = nameof(Data);
            var user = await GetCurrentUserAsync();
            return View(new DataViewModel() { Selected = selected, MyProfile = await GetCurrentUserAsync() });
        }
    }
}
