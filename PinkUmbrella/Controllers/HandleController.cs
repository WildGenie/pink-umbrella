using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Local;
using Tides.Services;

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    public class HandleController : BaseController
    {
        private readonly IHandleService _handles;
        public HandleController(IWebHostEnvironment environment, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService localProfiles, IPublicProfileService publicProfiles,
            IReactionService reactions, ITagService tags, INotificationService notifications, IPeerService peers, IAuthService auth,
            ISettingsService settings, IActivityStreamRepository activityStreams, IHandleService handles):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
            _handles = handles;
        }

        [Route("/Handle/Completions/{prefix}")]
        public async Task<IActionResult> Completions(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var tags = await _handles.GetCompletionsFor(prefix);
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
                return Json(!await _handles.HandleExists(handle));
            }
            else
            {
                return NotFound();
            }
        }
    }
}
