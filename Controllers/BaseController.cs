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

using PinkUmbrella.Models;
using PinkUmbrella.Services;

namespace PinkUmbrella.Controllers
{
    public class BaseController: Controller
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly SignInManager<UserProfileModel> _signInManager;
        protected readonly UserManager<UserProfileModel> _userManager;
        protected readonly IPostService _posts;
        protected readonly IUserProfileService _userProfiles;
        protected readonly IReactionService _reactions;
        protected readonly ITagService _tags;
        protected readonly INotificationService _notifications;
        
        public BaseController(IWebHostEnvironment environment, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles,
            IReactionService reactions, ITagService tags, INotificationService notifications)
        {
            _environment = environment;
            _signInManager = signInManager;
            _userManager = userManager;
            _posts = posts;
            _userProfiles = userProfiles;
            _reactions = reactions;
            _tags = tags;
            _notifications = notifications;
        }

        protected Task<UserProfileModel> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

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