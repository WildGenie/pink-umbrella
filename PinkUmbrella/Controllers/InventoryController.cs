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
using Poncho.Models;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerInventory)]
    public class InventoryController : BaseController
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly ISimpleResourceService _resources;
        private readonly ISimpleInventoryService _inventories;

        public InventoryController(IWebHostEnvironment environment, ILogger<InventoryController> logger,
            SignInManager<UserProfileModel> signInManager, UserManager<UserProfileModel> userManager,
            IPostService posts, IUserProfileService localProfiles, IPublicProfileService publicProfiles, ISimpleResourceService resourceService,
            ISimpleInventoryService inventories, IReactionService reactions, ITagService tags,
            INotificationService notifications, IPeerService peers, IAuthService auth,
            ISettingsService settings):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings)
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
                Resources = await _resources.QueryUser(user.UserId, user.UserId, null, new PaginationModel() { start = 0, count = 10 }),
                Inventories = await _inventories.GetForUser(user.UserId, user.UserId),
                AddResourceEnabled = await _settings.FeatureManager.IsEnabledAsync(nameof(FeatureFlags.FunctionInventoryNewResource)),
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
                Resources = await _resources.QueryUser(id, user?.UserId, null, new PaginationModel() { start = 0, count = 10 }),
                Inventories = await _inventories.GetForUser(id, user?.UserId),
                AddResourceEnabled = false,
            };
            return View("Index", model);
        }

        public async Task<IActionResult> IndexMore(string queryText, int start, int count)
        {
            var user = await GetCurrentUserAsync();
            return View(new IndexViewModel() {
                MyProfile = await GetCurrentUserAsync(),
                Resources = await _resources.QueryUser(user.UserId, user.UserId, queryText, new PaginationModel() { start = start, count = count })
            });
        }

        [Route("/Inventory/{id}")]
        public async Task<IActionResult> Inventory(int id, int? selected = null, string queryText = null)
        {
            ViewData["Controller"] = "Inventory";
            ViewData["Action"] = nameof(Index);
            
            var currentUser = await GetCurrentUserAsync();
            var inventory = await _inventories.Get(id, currentUser?.UserId);
            if (inventory == null) {
                return NotFound();
            }
            
            var inventories = await _inventories.GetForUser(inventory.OwnerUserId, currentUser?.UserId);

            var model = new IndexViewModel() {
                InventoryId = id,
                SelectedId = selected,
                MyProfile = await GetCurrentUserAsync(),
                Resources = await _resources.QueryInventory(id, currentUser?.UserId, queryText, new PaginationModel() { start = 0, count = 10 }),
                Inventories = inventories,
                Inventory = inventory,
                AddResourceEnabled = await _settings.FeatureManager.IsEnabledAsync(nameof(FeatureFlags.FunctionInventoryNewResource)) && currentUser?.UserId == inventory.OwnerUserId,
            };
            model.NewResource.Resource.InventoryId = id;
            return View("Inventory", model);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> NewResource(SimpleResourceModel Resource)
        {
            var user = await GetCurrentUserAsync();
            Resource.CreatedByUserId = user.UserId;
            var result = await _resources.CreateResource(Resource);

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
                MyProfile = user,
                Inventory = new SimpleInventoryModel()
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> NewInventory(SimpleInventoryModel Inventory)
        {
            var user = await GetCurrentUserAsync();
            Inventory.OwnerUserId = user.UserId;
            var result = await _inventories.CreateInventory(Inventory);

            if (result != null) {
                return RedirectToAction(nameof(Inventory), new { Id = Inventory.Id });
            } else {
                return View(new NewInventoryViewModel() {
                    MyProfile = user,
                    Inventory = Inventory
                });
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
