using System;
using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Util;
using Tides.Models.Auth;

namespace PinkUmbrella.Controllers
{
    public partial class PersonController: ActivityStreamController
    {

        [AllowAnonymous]
        [FeatureGate(FeatureFlags.ApiControllerPerson)]
        [Route("/Api/Person")]
        public async Task<ActionResult> ApiIndex()
        {
            return Json(new Peer()
            {
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
        [FeatureGate(FeatureFlags.ApiControllerPerson)]
        [Route("/Api/Person/All")]
        public async Task<ActionResult> All(DateTime? sinceLastUpdated = null)
        {
            var users = await _localProfiles.GetAll(sinceLastUpdated);
            return Json(new
            {
                profiles = users.Select(u => u.ToElastic()).ToArray()
            });
        }
    }
}