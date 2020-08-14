using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Services;
using Estuary.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services;
using PinkUmbrella.Util;
using Tides.Models.Auth;
using Tides.Models.Peer;

namespace PinkUmbrella.Controllers.Api
{
    [FeatureGate(FeatureFlags.ApiControllerSystem)]
    [ServiceFilter(typeof(ApiCallFilterAttribute))]
    [Route("/Api/[controller]/[action]"), ApiController]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(SystemController))]
    [ApiVersionNeutral]
    public class SystemController: Controller
    {
        private readonly IAuthService _auth;
        private readonly IPeerService _peers;
        private readonly SimpleDbContext _db;
        private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
        private readonly IActivityStreamRepository _activityStreams;
        
        public SystemController(IAuthService auth, SimpleDbContext db, IPeerService peers, IApiDescriptionGroupCollectionProvider apiExplorer, IActivityStreamRepository activityStreams)
        {
            _auth = auth;
            _db = db;
            _peers = peers;
            _apiExplorer = apiExplorer;
            _activityStreams = activityStreams;
        }

        [AllowAnonymous]
        [Produces("application/json", "application/pink-umbrella")]
        [ProducesResponseType(typeof(Peer), 200)]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return Json(new Peer() {
                name = "Hello World",
                Address = new IPAddressModel()
                {
                    Name = HttpContext.Request.Host.Host,
                },
                AddressPort = HttpContext.Request.Host.Port ?? 443,
                PublicKey = await _auth.GetCurrent(),
            });
        }

        [AllowAnonymous]
        [Produces("application/json", "application/pink-umbrella")]
        [ProducesResponseType(typeof(PeerStatsModel), 200)]
        [HttpGet]
        public async Task<ActionResult> Stats()
        {
            return Json(new PeerStatsModel() {
                PeerCount = await _peers.CountAsync(),
                MediaCount = await _db.ArchivedMedia.CountAsync(),
                PostCount = int.Parse((await _activityStreams.Get(new ActivityStreamFilter("outbox") { peerId = 0, countOnly = true }.FixObjType("Note", "Article"))).summary),
                ShopCount = int.Parse((await _activityStreams.Get(new ActivityStreamFilter("outbox") { peerId = 0, countOnly = true }.FixObjType("Organization"))).summary),
                UserCount = await _db.Users.CountAsync(),
                StartTime = Process.GetCurrentProcess().StartTime,
            });
        }

        [AllowAnonymous]
        [Produces("application/json", "application/pink-umbrella")]
        [ProducesResponseType(typeof(ApiListModel), 200)]
        [HttpGet]
        public async Task<ActionResult> Api(string filter)
        {
            var actions = _apiExplorer.ApiDescriptionGroups.Items.SelectMany(api => api.Items).Select(action => new ApiListModel
            {
                Method = action.HttpMethod,
                Route = action.RelativePath,
            });
            if (!string.IsNullOrWhiteSpace(filter) && filter.Length < 255)
            {
                filter = filter.ToLower();
                actions = actions.Where(e => e.Route.ToLower().Contains(filter));
            }
            await Task.Delay(1);
            return Json(new
            {
                Items = actions.ToList()
            });
        }
    }
}