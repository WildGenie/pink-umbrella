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
using PinkUmbrella.ViewModels.Debug;

namespace PinkUmbrella.Controllers
{
    public class DebugController : BaseController
    {
        private readonly ILogger<DebugController> _logger;
        private readonly IDebugService _debugService;
        private readonly RoleManager<UserGroupModel> _roleManager;

        public DebugController(IWebHostEnvironment environment, ILogger<DebugController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IDebugService debugService,
            RoleManager<UserGroupModel> roleManager):
            base(environment, signInManager, userManager, posts, userProfiles)
        {
            _logger = logger;
            _debugService = debugService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Debug";
                ViewData["Action"] = nameof(Index);
                return View(new IndexViewModel() {
                    MyProfile = user,
                    UnusedUnexpiredAccessCodes = await _userProfiles.GetUnusedUnexpiredAccessCodes(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Users()
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Debug";
                ViewData["Action"] = nameof(Users);
                return View(new UsersViewModel() {
                    MyProfile = user,
                    MostRecentlyCreatedUsers = await _userProfiles.GetMostRecentlyCreatedUsers(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Exceptions(PaginationModel pagination)
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Debug";
                ViewData["Action"] = nameof(Exceptions);
                return View(new ExceptionsViewModel() {
                    MyProfile = user,
                    Exceptions = await _debugService.Get(pagination)
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> ThrowException()
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                throw new Exception("You threw this exception");
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Posts()
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Debug";
                ViewData["Action"] = nameof(Posts);
                return View(new PostsViewModel() {
                    MyProfile = user,
                    MostReportedPosts = await _posts.GetMostReportedPosts(),
                    MostBlockedPosts = await _posts.GetMostBlockedPosts(),
                    MostDislikedPosts = await _posts.GetMostDislikedPosts(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        public async Task<IActionResult> Community()
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                ViewData["Controller"] = "Debug";
                ViewData["Action"] = nameof(Community);
                return View();
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiveAccessToGroup()
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                return View();
            }
            else
            {
                return Redirect("/Error/404");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> GiveAccessToGroup(int toUserId, string group)
        {
            var user = await GetCurrentUserAsync();
            if (await _userManager.IsInRoleAsync(user, "dev"))
            {
                if (await _roleManager.RoleExistsAsync(group))
                {
                    var code = await _userProfiles.NewGroupAccessCode(user.Id, toUserId, group);
                    return Content($"You have given {toUserId} access to {group}. The link is\n<a href=\"/AddMeToGroup/{code.Code}\">{code.Code}</a>");
                }
            }
            
            return Redirect("/Error/404");
        }
    }
}
