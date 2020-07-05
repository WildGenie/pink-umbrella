using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Util;

namespace PinkUmbrella.Controllers
{
    public partial class ProfileController: BaseController
    {

        [AllowAnonymous]
        [FeatureGate(FeatureFlags.ApiControllerProfile)]
        [Route("/Api/Profile")]
        public async Task<ActionResult> ApiIndex()
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
        [FeatureGate(FeatureFlags.ApiControllerProfile)]
        [Route("/Api/Profile/All")]
        public async Task<ActionResult> All(DateTime? sinceLastUpdated = null)
        {
            var users = await _userProfiles.GetAll(sinceLastUpdated);
            return Json(new {
                profiles = users.Select(u => u.ToElastic()).ToArray()
            });
        }
    }
}