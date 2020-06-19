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
    public class TagController : BaseController
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IShopService _shops;

        public TagController(IWebHostEnvironment environment, ILogger<ShopController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IShopService shops,
            IReactionService reactions, ITagService tags):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags)
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
                    Shop = await _shops.GetShopByHandle(handle, user?.Id)
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
    }
}
