using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels;
using PinkUmbrella.ViewModels.Shop;
using PinkUmbrella.ViewModels.Shared;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Local;
using Tides.Models;
using Estuary.Core;
using Estuary.Services;
using Estuary.Activities;
using Estuary.Util;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerShop)]
    public class ShopController : ActivityStreamController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;
        
        public ShopController(
            IWebHostEnvironment environment,
            ILogger<ShopController> logger,
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IPostService posts,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            IShopService shops, 
            IReactionService reactions,
            ITagService tags,
            INotificationService notifications,
            IPeerService peers,
            IAuthService auth,
            ISettingsService settings,
            IActivityStreamRepository activityStreams):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
            _logger = logger;
            _shops = shops;
        }

        [AllowAnonymous, Route("/Shop/{handle?}")]
        public async Task<IActionResult> Index(string handle = null)
        {
            ViewData["Controller"] = "Shop";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            
            if (!string.IsNullOrWhiteSpace(handle))
            {
                var model = new ShopViewModel() {
                    MyProfile = user,
                    Shop = await _activityStreams.Get(new ActivityStreamFilter("outbox")
                    {
                        handle = handle,
                        viewerId = user?.UserId
                    }.FixObjType(nameof(Estuary.Actors.Common.Organization)))
                };
                return View("Shop", model);
            }
            else
            {
                var topTags = await _tags.GetMostUsedTagsForSubject("Shop");
                if (topTags.totalItems > 0)
                {
                    var shopsByCategory = new Dictionary<int, BaseObject>();
                    foreach (var category in topTags.items)
                    {
                        shopsByCategory[category.objectId.Value] = await _shops.GetShopsTaggedUnder(category, user?.UserId);
                    }
                    var model = new IndexViewModel() {
                        MyProfile = user,
                        ShopsByCategory = shopsByCategory,
                        Categories = topTags
                    };
                    return View(model);
                }
                else
                {
                    var allShops = await _activityStreams.Get(new ActivityStreamFilter("outbox")
                    {
                        viewerId = user?.UserId
                    }.FixObjType(nameof(Estuary.Actors.Common.Organization)));
                    
                    ListViewModel lvm = null;
                    if (allShops is CollectionObject collection)
                    {
                        lvm = new ListViewModel
                        {
                            Items = collection,
                            EmptyViewName = "BeTheFirst",
                        };
                    }

                    var model = new IndexViewModel()
                    {
                        MyProfile = user,
                        ListView = lvm
                    };
                    lvm.EmptyViewModel = model;
                    return View(model);
                }
            }
        }

        public async Task<IActionResult> Tags()
        {
            ViewData["Controller"] = "Shop";
            ViewData["Action"] = nameof(Tags);
            var user = await GetCurrentUserAsync();
            return View(new TagsViewModel() {
                MyProfile = user,
                Tags = await _tags.GetTagsForSubject("Shop", user?.UserId),
                Type = SearchResultType.Shop,
            });
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            ViewData["Controller"] = "Shop";
            ViewData["Action"] = nameof(New);
            var user = await GetCurrentUserAsync();
            await GetShopTagsDebugValue();
            return View(new NewShopViewModel() {
                MyProfile = user
            });
        }

        public class SimpleTag
        {
            public string label { get; set; }
            public int value { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> New(NewShopViewModel model)
        {
            var user = await GetCurrentUserAsync();
            var shop = await model.Validate(this.ModelState, user.objectId.Value, _tags);
            shop.attributedTo.Add(user);

            if (this.ModelState.ErrorCount == 0)
            {
                var error = await _shops.TryCreateShop(shop);
                if (error != null)
                {
                    this.ModelState.AddModelError(error.name, error.summary);
                }
            }

            if (this.ModelState.ErrorCount == 0)
            {
                return RedirectToAction(nameof(Index), new { handle = shop.Handle });
            }
            else
            {
                await GetShopTagsDebugValue();
                ViewData["Controller"] = "Shop";
                ViewData["Action"] = nameof(New);
                model.MyProfile = user;
                return View(model);
            }
        }

        private async Task GetShopTagsDebugValue()
        {
            if (Debugger.IsAttached)
            {
                var debug = new List<BaseObject>();
                foreach (var tmp in new string[] {})
                {
                    var t = await _tags.GetTag("", null);
                    if (t != null)
                    {
                        debug.Add(t);
                    }
                }

                if (debug.Count == 0 && Debugger.IsAttached)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        debug.Add(new BaseObject { id = "tag/-1", objectId = -1, content = $"TestTag{i}" });
                    }
                }
                ViewData["ShopTagsDebugValue"] = debug;
            }
        }
    }
}
