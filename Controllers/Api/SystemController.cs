using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services;
using PinkUmbrella.Util;

namespace PinkUmbrella.Controllers.Api
{
    [FeatureGate(FeatureFlags.ApiControllerSystem)]
    [ServiceFilter(typeof(ApiCallFilterAttribute))]
    [Route("/Api/[controller]/[action]"), ApiController]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(SystemController))]
    public class SystemController: Controller
    {
        private readonly IAuthService _auth;
        private readonly IPeerService _peers;
        private readonly SimpleDbContext _db;
        
        public SystemController(IAuthService auth, SimpleDbContext db, IPeerService peers)
        {
            _auth = auth;
            _db = db;
            _peers = peers;
        }

        [AllowAnonymous]
        [Produces("application/json", "application/pink-umbrella")]
        [ProducesResponseType(typeof(PeerModel), 200)]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return Json(new {
                Model = new PeerModel() {
                    DisplayName = "Hello World",
                    Address = new IPAddressModel()
                    {
                        Name = HttpContext.Request.Host.Host,
                    },
                    AddressPort = HttpContext.Request.Host.Port ?? 443,
                    //PublicKey = await _auth.GetCurrent(),
                }
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
                PostCount = await _db.Posts.CountAsync(),
                ShopCount = await _db.Shops.CountAsync(),
                UserCount = await _db.Users.CountAsync(),
                StartTime = Process.GetCurrentProcess().StartTime,
            });
        }
    }
}