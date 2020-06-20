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
using PinkUmbrella.ViewModels.Inventory;

namespace PinkUmbrella.Controllers
{
    public class InventoryController : BaseController
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly ISimpleResourceService _resources;
        private readonly ISimpleInventoryService _inventories;

        public InventoryController(IWebHostEnvironment environment, ILogger<InventoryController> logger,
            SignInManager<UserProfileModel> signInManager, UserManager<UserProfileModel> userManager,
            IPostService posts, IUserProfileService userProfiles, ISimpleResourceService resourceService,
            ISimpleInventoryService inventories, IReactionService reactions, ITagService tags):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags)
        {
            _logger = logger;
            _resources = resourceService;
            _inventories = inventories;
        }

        [Route("/Inventory"), Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Inventory";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user,
                Resources = await _resources.QueryUser(user.Id, user.Id, null, new PaginationModel() { start = 0, count = 10 }),
                Inventories = await _inventories.GetForUser(user.Id, user.Id),
                AddResourceEnabled = true,
            };
            model.NewResource.AvailableBrands = await _resources.GetBrands();
            model.NewResource.AvailableCategories = await _resources.GetCategories();
            model.NewResource.AvailableUnits = await _resources.GetUnits();
            return View(model);
        }

        [Route("/Inventory/Profile/{id}")]
        public async Task<IActionResult> Profile(int id)
        {
            ViewData["Controller"] = "Inventory";
            ViewData["Action"] = nameof(Profile);
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user,
                Resources = await _resources.QueryUser(id, user?.Id, null, new PaginationModel() { start = 0, count = 10 }),
                Inventories = await _inventories.GetForUser(id, user?.Id),
                AddResourceEnabled = false,
            };
            return View("Index", model);
        }

        public async Task<IActionResult> IndexMore(string queryText, int start, int count)
        {
            var user = await GetCurrentUserAsync();
            return View(new IndexViewModel() {
                MyProfile = await GetCurrentUserAsync(),
                Resources = await _resources.QueryUser(user.Id, user.Id, queryText, new PaginationModel() { start = start, count = count })
            });
        }

        [Route("/Inventory/{id}")]
        public async Task<IActionResult> Inventory(int id, int? selected = null, string queryText = null)
        {
            ViewData["Controller"] = "Inventory";
            ViewData["Action"] = nameof(Index);
            
            var currentUser = await GetCurrentUserAsync();
            var inventory = await _inventories.Get(id, currentUser?.Id);
            if (inventory == null) {
                return NotFound();
            }
            
            var inventories = await _inventories.GetForUser(inventory.OwnerUserId, currentUser?.Id);

            var model = new IndexViewModel() {
                InventoryId = id,
                SelectedId = selected,
                MyProfile = await GetCurrentUserAsync(),
                Resources = await _resources.QueryInventory(id, currentUser?.Id, queryText, new PaginationModel() { start = 0, count = 10 }),
                Inventories = inventories,
                Inventory = inventory,
                AddResourceEnabled = currentUser?.Id == inventory.OwnerUserId,
            };
            model.NewResource.Resource.InventoryId = id;
            return View("Inventory", model);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> NewResource(SimpleResourceModel Resource)
        {
            var user = await GetCurrentUserAsync();
            Resource.CreatedByUserId = user.Id;
            var result = await _resources.CreateResource(Resource);

            if (result != null) {
                return RedirectToAction(nameof(Index), new { Id = Resource.InventoryId, Selected = Resource.Id });
            } else {
                return View(new NewResourceViewModel() {
                    Resource = Resource,
                    AvailableBrands = await _resources.GetBrands(),
                    AvailableCategories = await _resources.GetCategories(),
                    AvailableUnits = await _resources.GetUnits()
                });   
            }
        }

        [HttpPost, Authorize]
        public Task<IActionResult> NewResourceIndex([Bind(Prefix="NewResource.Resource")] SimpleResourceModel Resource)
        {
            return NewResource(Resource);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> NewInventory()
        {
            var user = await GetCurrentUserAsync();
            return View(new SimpleInventoryModel());
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> NewInventory(SimpleInventoryModel Inventory)
        {
            var user = await GetCurrentUserAsync();
            Inventory.OwnerUserId = user.Id;
            var result = await _inventories.CreateInventory(Inventory);

            if (result != null) {
                return RedirectToAction(nameof(Inventory), new { Id = Inventory.Id });
            } else {
                return View(Inventory);   
            }
        }

        public async Task<IActionResult> Resource(int id)
        {
            ViewData["Controller"] = "Inventory";
            ViewData["Action"] = nameof(Resource);
            var user = await GetCurrentUserAsync();
            return View(new ResourceViewModel() {
                MyProfile = await GetCurrentUserAsync(),
                Resource = await _resources.GetResource(id, user?.Id),
                ReturnUrl = Request.Headers["Referer"].ToString()
            });
        }
    }
}
