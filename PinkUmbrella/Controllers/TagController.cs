using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Services.Local;
using PinkUmbrella.ViewModels.Tag;

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    [FeatureGate(FeatureFlags.ControllerTag)]
    public class TagController: BaseController
    {
        private readonly ILogger<TagController> _logger;
        private readonly ITagService _tags;

        public TagController(
            ILogger<TagController> logger,
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            ITagService tags,
            IAuthService auth,
            ISettingsService settings):
            base(signInManager, userManager, localProfiles, publicProfiles, auth, settings)
        {
            _logger = logger;
            _tags = tags;
        }

        [Route("/Tag/{handle}")]
        public async Task<IActionResult> Index(string handle = null)
        {
            ViewData["Controller"] = "Tag";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            
            if (!string.IsNullOrWhiteSpace(handle))
            {
                var tag = await _tags.GetTag(handle, user?.UserId);
                if (tag != null)
                {
                    var model = new TagViewModel() {
                        MyProfile = user,
                        Tag = tag
                    };
                    return View(model);
                }
            }
            return NotFound();
        }

        [Route("/Tag/Completions/{prefix}")]
        public async Task<IActionResult> Completions(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var tags = await _tags.GetCompletionsForTag(prefix);
                return Json(new {
                    items = tags.items.Select(t => new { value = t.objectId?.ToString(), label = t.content }).ToArray()
                });
            }
            else
            {
                return NotFound();
            }
        }
    }
}
