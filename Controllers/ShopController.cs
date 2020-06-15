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

namespace PinkUmbrella.Controllers
{
    [AllowAnonymous]
    public class ShopController : BaseController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;

        public ShopController(IWebHostEnvironment environment, ILogger<ShopController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IShopService shops, IReactionService reactions):
            base(environment, signInManager, userManager, posts, userProfiles, reactions)
        {
            _logger = logger;
            _shops = shops;
        }

        //[Route("/Shop/{handle}")]
        public async Task<IActionResult> Index(string handle = null)
        {
            ViewData["Controller"] = "Shop";
            ViewData["Action"] = nameof(Index);
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
