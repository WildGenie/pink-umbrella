using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Services;

namespace PinkUmbrella.Controllers.Api
{
    public class SystemController: Controller
    {
        private readonly IAuthService _auth;
        
        public SystemController(IAuthService auth)
        {
            _auth = auth;
        }

        [AllowAnonymous]
        public async Task<ActionResult> IndexAsync()
        {
            return Json(new PeerModel() {
                DisplayName = "Hello World",
                Handle = HttpContext.Request.Host.Value,
                Auth = await _auth.GetCurrent(),
            });
        }
    }
}