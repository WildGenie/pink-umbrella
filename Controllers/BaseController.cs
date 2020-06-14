using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Logging;

using seattle.Models;
using seattle.Services;

namespace seattle.Controllers
{
    public class BaseController: Controller
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly SignInManager<UserProfileModel> _signInManager;
        protected readonly UserManager<UserProfileModel> _userManager;
        protected readonly IPostService _posts;
        protected readonly IUserProfileService _userProfiles;
        
        public BaseController(IWebHostEnvironment environment, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles)
        {
            _environment = environment;
            _signInManager = signInManager;
            _userManager = userManager;
            _posts = posts;
            _userProfiles = userProfiles;
        }

        protected Task<UserProfileModel> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        protected bool IsAjaxRequest() {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return true;
            }
            else
            {
                return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }
        }
    }
}