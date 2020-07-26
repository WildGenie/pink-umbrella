using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Shop;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Services.Local;
using Tides.Services;
using Tides.Models;

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    [FeatureGate(FeatureFlags.ControllerTag)]
    public class TagController : BaseController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;

        public TagController(IWebHostEnvironment environment, ILogger<ShopController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService localProfiles, IPublicProfileService publicProfiles, IShopService shops,
            IReactionService reactions, ITagService tags, INotificationService notifications, IPeerService peers, IAuthService auth,
            ISettingsService settings, IActivityStreamRepository activityStreams):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
            _logger = logger;
            _shops = shops;
        }

        //[Route("/Shop/{handle}")]
        public async Task<IActionResult> Index(string handle = null)
        {
            ViewData["Controller"] = "Tag";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            
            if (!string.IsNullOrWhiteSpace(handle))
            {
                var model = new ShopViewModel() {
                    MyProfile = user,
                    Shop = await _activityStreams.GetShop(new ActivityStreamFilter
                    {
                        handle = handle, userId = user?.objectId, viewerId = user?.objectId
                    })
                };
                return View(model);
            }
            else
            {
                var model = new IndexViewModel() {

                };
                return View(model);
            }
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
