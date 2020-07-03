using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Elastic;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services;
using PinkUmbrella.Util;

namespace PinkUmbrella.Controllers.Api
{
    [FeatureGate(FeatureFlags.ApiControllerSystem)]
    [Route("api/[controller]/[action]"), ApiController]
    public class ProfileController: Controller
    {
        private readonly IAuthService _auth;
        private readonly IPeerService _peers;
        private readonly SimpleDbContext _db;
        private readonly IUserProfileService _profiles;
        
        public ProfileController(IAuthService auth, SimpleDbContext db, IPeerService peers, IUserProfileService profiles)
        {
            _auth = auth;
            _db = db;
            _peers = peers;
            _profiles = profiles;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            return Json(new PeerModel() {
                DisplayName = "Hello World",
                Address = new IPAddressModel()
                {
                    Name = HttpContext.Request.Host.Host,
                },
                AddressPort = HttpContext.Request.Host.Port ?? 443,
                PublicKey = await _auth.GetCurrent(),
            });
        }

        [AllowAnonymous]
        public async Task<ActionResult> All(DateTime? sinceLastUpdated = null)
        {
            var users = await _profiles.GetAll(sinceLastUpdated);
            return Json(new {
                profiles = users.Select(u => u.ToElastic()).ToArray()
            });
        }
    }
}