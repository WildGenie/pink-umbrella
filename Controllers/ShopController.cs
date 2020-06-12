using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using seattle.Models;
using seattle.Services;
using seattle.ViewModels.Shop;

namespace seattle.Controllers
{
    public class ShopController : BaseController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;

        public ShopController(IWebHostEnvironment environment, ILogger<ShopController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IFeedService feeds, IUserProfileService userProfiles, IShopService shops):
            base(environment, signInManager, userManager, feeds, userProfiles)
        {
            _logger = logger;
            _shops = shops;
        }

        //[Route("/Shop/{handle}")]
        public async Task<IActionResult> Index(string handle = null)
        {
            var user = await GetCurrentUserAsync();
            var model = new ShopViewModel() {
                MyProfile = user
            };
            
            if (!string.IsNullOrWhiteSpace(handle))
            {
                model.Shop = await _shops.GetShopByHandle(handle);
            }

            return View(model);
        }
    }
}
