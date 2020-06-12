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
using seattle.ViewModels.Inventory;

namespace seattle.Controllers
{
    public class InventoryController : BaseController
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly ISimpleResourceService _resources;
        private readonly ISimpleInventoryService _inventories;

        public InventoryController(IWebHostEnvironment environment, ILogger<InventoryController> logger,
            SignInManager<UserProfileModel> signInManager, UserManager<UserProfileModel> userManager,
            IFeedService feeds, IUserProfileService userProfiles, ISimpleResourceService resourceService,
            ISimpleInventoryService inventories):
            base(environment, signInManager, userManager, feeds, userProfiles)
        {
            _logger = logger;
            _resources = resourceService;
            _inventories = inventories;
        }

        public async Task<IActionResult> Index(string queryText)
        {
            var user = await GetCurrentUserAsync();
            return View(new IndexViewModel() {
                MyProfile = await _userProfiles.GetUser(0),
                Resources = await _resources.QueryInventory(user.Id, -1, queryText, new PaginationModel() { start = 0, count = 10 }),
                Inventories = await _inventories.GetForUser(user.Id)
            });
        }

        public async Task<IActionResult> IndexMore(string queryText, int start, int count)
        {
            var user = await GetCurrentUserAsync();
            return View(new IndexViewModel() {
                MyProfile = await _userProfiles.GetUser(0),
                Resources = await _resources.QueryInventory(user.Id, -1, queryText, new PaginationModel() { start = start, count = count })
            });
        }
    }
}
