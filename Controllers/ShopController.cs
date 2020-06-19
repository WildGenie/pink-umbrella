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
using PinkUmbrella.ViewModels;

namespace PinkUmbrella.Controllers
{
    public class ShopController : BaseController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;

        public ShopController(IWebHostEnvironment environment, ILogger<ShopController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IShopService shops, 
            IReactionService reactions, ITagService tags):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags)
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
                    Shop = await _shops.GetShopByHandle(handle, user?.Id)
                };
                return View("Shop", model);
            }
            else
            {
                var topTags = await _tags.GetMostUsedTagsForSubject(ReactionSubject.Shop);
                if (topTags.Total > 0)
                {
                    var shopsByCategory = new Dictionary<int, List<ShopModel>>();
                    foreach (var category in topTags.Items)
                    {
                        shopsByCategory[category.Tag.Id] = await _shops.GetShopsTaggedUnder(category.Tag, user?.Id);
                    }
                    var model = new IndexViewModel() {
                        ShopsByCategory = shopsByCategory,
                        Categories = topTags
                    };
                    return View(model);
                }
                else
                {
                    var allShops = await _shops.GetAllShops(user?.Id);
                    if (allShops.Count > 0)
                    {
                        var model = new IndexViewModel() {
                            ShopsList = allShops
                        };
                        return View(model);
                    }
                    else
                    {
                        return View("BeTheFirst", new BaseViewModel() { MyProfile = user });
                    }
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            ViewData["Controller"] = "Shop";
            ViewData["Action"] = nameof(New);
            var user = await GetCurrentUserAsync();
            return View(new NewShopViewModel() {
                MyProfile = user
            });
        }

        [HttpPost]
        public async Task<IActionResult> New(NewShopViewModel model)
        {
            var user = await GetCurrentUserAsync();
            var shop = model.Validate(this.ModelState);
            shop.UserId = user.Id;
            var error = await _shops.TryCreateShop(shop);
            if (error != null)
            {
                this.ModelState.AddModelError(error.ParamName, error.Message);
            }
            if (this.ModelState.ErrorCount == 0)
            {
                return RedirectToAction(nameof(Index), new { handle = shop.Handle });
            }
            else
            {
                ViewData["Controller"] = "Shop";
                ViewData["Action"] = nameof(New);
                model.MyProfile = user;
                return View(model);
            }
        }
    }
}
