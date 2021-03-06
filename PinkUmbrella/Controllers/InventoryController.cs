using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Inventory;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Services.Local;
using Tides.Models;
using Estuary.Core;
using Estuary.Services;
using Estuary.Util;
using PinkUmbrella.ViewModels.Shared;
using System;
using Tides.Models.Public;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerInventory)]
    public class InventoryController : ActivityStreamController
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly ISimpleResourceService _resources;
        private readonly ISimpleInventoryService _inventories;

        public InventoryController(
            IWebHostEnvironment environment,
            ILogger<InventoryController> logger,
            SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager,
            IPostService posts,
            IUserProfileService localProfiles,
            IPublicProfileService publicProfiles,
            ISimpleResourceService resourceService,
            ISimpleInventoryService inventories,
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
            _resources = resourceService;
            _inventories = inventories;
        }

        [Route("/Inventory"), Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["Controller"] = "Inventory";
            ViewData["Action"] = nameof(Index);
            var user = await GetCurrentUserAsync();

            var resources = ListViewModel.Links(await _activityStreams.Get(new ActivityStreamFilter("outbox")
                {
                    viewerId = user?.UserId
                }.FixObjType("Resource"))); // await _resources.QueryUser(user.UserId.Value, user.UserId, null, new PaginationModel() { start = 0, count = 10 })

            var model = new IndexViewModel() {
                MyProfile = user,
                Resources = resources,
                Inventories = null,//await _inventories.GetForUser(user.UserId.Value, user.UserId),
                AddResourceEnabled = await _settings.FeatureManager.IsEnabledAsync(nameof(FeatureFlags.FunctionInventoryNewResource)),
            };
            model.NewResource.AvailableBrands = await _resources.GetBrands();
            model.NewResource.AvailableCategories = await _resources.GetCategories();
            model.NewResource.AvailableUnits = await _resources.GetUnits();
            return View(model);
        }

        //[Route("/Inventory/Person/{id}")]
        // public async Task<IActionResult> Person(int id)
        // {
        //     ViewData["Controller"] = "Inventory";
        //     ViewData["Action"] = nameof(Person);
        //     var user = await GetCurrentUserAsync();
        //     var model = new IndexViewModel() {
        //         MyProfile = user,
        //         Resources = await _resources.QueryUser(id, user?.UserId, null, new PaginationModel() { start = 0, count = 10 }),
        //         Inventories = await _inventories.GetForUser(id, user?.UserId),
        //         AddResourceEnabled = false,
        //     };
        //     return View("Index", model);
        // }

        // public async Task<IActionResult> IndexMore(string queryText, int start, int count)
        // {
        //     var user = await GetCurrentUserAsync();
        //     return View(new IndexViewModel() {
        //         MyProfile = await GetCurrentUserAsync(),
        //         Resources = await _resources.QueryUser(user.UserId.Value, user.UserId, queryText, new PaginationModel() { start = start, count = count })
        //     });
        // }

        [Route("/Inventory/{id}")]
        public async Task<IActionResult> Inventory(string id, string selected = null, string queryText = null)
        {
            ViewData["Controller"] = "Inventory";
            ViewData["Action"] = nameof(Index);
            
            var currentUser = await GetCurrentUserAsync();
            var inventory = await _activityStreams.Get(new ActivityStreamFilter("outbox")
            {
                id = new PublicId(id), viewerId = currentUser?.UserId
            }.FixObjType("Inventory"));

            if (inventory == null)
            {
                return NotFound();
            }
            
            var inventories = await GetInventories(inventory, currentUser);

            var resources = ListViewModel.Links(await _activityStreams.Get(new ActivityStreamFilter("outbox")
            {
                id = new PublicId(id), viewerId = currentUser?.UserId
            }.FixObjType("Resource")));
            resources.SelectedId = selected;
            
            // queryText
            var model = new IndexViewModel() {
                InventoryId = id,
                MyProfile = await GetCurrentUserAsync(),
                Resources = resources,
                Inventories = inventories,
                Inventory = inventory,
                AddResourceEnabled = await _settings.FeatureManager.IsEnabledAsync(nameof(FeatureFlags.FunctionInventoryNewResource)) && currentUser?.UserId == inventory.UserId,
            };
            // TODO: uncomment
            //model.NewResource.Resource.InventoryId = id;
            return View("Inventory", model);
        }

        private async Task<ListViewModel> GetInventories(BaseObject inventory, Estuary.Actors.ActorObject currentUser)
        {
            var lvm = ListViewModel.Links(await _inventories.GetForUser(inventory.UserId.Value, currentUser?.UserId));
            lvm.SelectedId = inventory.id;
            return lvm;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> NewResource(SimpleResourceModel Resource)
        {
            var user = await GetCurrentUserAsync();
            Resource.CreatedByUserId = user.UserId.Value;
            var result = await _resources.CreateResource(await _resources.Transform(Resource));

            if (result != null) {
                return RedirectToAction(nameof(Index), new { Id = Resource.InventoryId, Selected = Resource.Id });
            } else {
                return View(new NewResourceViewModel() {
                    MyProfile = user,
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
            return View(new NewInventoryViewModel() {
                MyProfile = user
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> NewInventory(NewInventoryViewModel Inventory)
        {
            var user = await GetCurrentUserAsync();
            Inventory.OwnerUserId = user.UserId.Value;
            var result = await _inventories.CreateInventory(_inventories.Transform(Inventory));

            if (result != null) {
                return RedirectToAction(nameof(Inventory), new { Id = Inventory.Id });
            } else {
                Inventory.MyProfile = user;
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
                Resource = await _resources.GetResource(id, user?.UserId),
                ReturnUrl = Request.Headers["Referer"].ToString()
            });
        }
    }
}
