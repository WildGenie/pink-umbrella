using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Local;

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    public class HandleController : BaseController
    {
        private readonly IObjectReferenceService _handles;
        public HandleController(
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            IAuthService auth,
            ISettingsService settings,
            IObjectReferenceService handles):
            base(signInManager, userManager, localProfiles, publicProfiles, auth, settings)
        {
            _handles = handles;
        }

        [Route("/Handle/Completions/{prefix}")]
        public async Task<IActionResult> Completions(string prefix, string type)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var tags = await _handles.GetCompletionsFor(prefix, type);
                return Json(new {
                    items = tags.Select(t => new { value = t.Handle, label = t.Handle }).ToArray()
                });
            }
            else
            {
                return NotFound();
            }
        }

        [Route("/Handle/IsUnique/{prefix}")]
        public async Task<IActionResult> IsHandleUnique([FromQuery(Name="Shop.Handle")] string handle)
        {
            if (!string.IsNullOrWhiteSpace(handle))
            {
                return Json(!await _handles.HandleExists(handle, "Organization"));
            }
            else
            {
                return NotFound();
            }
        }
    }
}
