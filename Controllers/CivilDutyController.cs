using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;

namespace PinkUmbrella.Controllers
{
    public class CivicDutyController : BaseController
    {
        private readonly ILogger<CivicDutyController> _logger;

        public CivicDutyController(IWebHostEnvironment environment, ILogger<CivicDutyController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles):
            base(environment, signInManager, userManager, posts, userProfiles)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Controller"] = "CivicDuty";
            ViewData["Action"] = nameof(Index);
            return View();
        }
    }
}
