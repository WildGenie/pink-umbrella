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
using PinkUmbrella.ViewModels.Admin;
using PinkUmbrella.Models.AhPushIt;
using PinkUmbrella.Models.Auth;
using System.Net;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Models.Settings;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Util;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerAdmin)]
    [AllowAnonymous, IsAdminOrDebuggingOrElse404Filter]
    public class AdminController : BaseController
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IDebugService _debugService;
        private readonly RoleManager<UserGroupModel> _roleManager;

        public AdminController(IWebHostEnvironment environment, ILogger<AdminController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService userProfiles, IDebugService debugService,
            RoleManager<UserGroupModel> roleManager, IReactionService reactions, ITagService tags, INotificationService notifications,
            IPeerService peers, IAuthService auth, ISettingsService settings):
            base(environment, signInManager, userManager, posts, userProfiles, reactions, tags, notifications, peers, auth, settings)
        {
            _logger = logger;
            _debugService = debugService;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "admin"))
            {
                ViewData["Controller"] = "Admin";
                ViewData["Action"] = nameof(Index);
                return View(new IndexViewModel()
                {
                    MyProfile = user,
                    UnusedUnexpiredAccessCodes = await _userProfiles.GetUnusedUnexpiredAccessCodes(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "admin"))
            {
                ViewData["Controller"] = "Admin";
                ViewData["Action"] = nameof(Users);
                return View(new UsersViewModel()
                {
                    MyProfile = user,
                    MostRecentlyCreatedUsers = await _userProfiles.GetMostRecentlyCreatedUsers(),
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Peers(PaginationModel pagination)
        {
            var user = await GetCurrentUserAsync();
            ViewData["Controller"] = "Admin";
            ViewData["Action"] = nameof(Peers);

            var peers = new List<PeerViewModel>();
            foreach (var p in await _peers.GetPeers())
            {
                var client = await _peers.Open(p.Address, p.AddressPort);
                peers.Add(new PeerViewModel()
                {
                    Peer = p,
                    Stats = await client.QueryStats(),
                });
            }

            return View(new PeersViewModel()
            {
                MyProfile = user,
                Peers = peers // pagination
            });
        }

        [HttpGet]
        public async Task<IActionResult> PreviewPeer(string url, IPAddressModel Addr, int? port)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var addresses = await Dns.GetHostAddressesAsync(new Uri(url).Authority);
                if (addresses.Length > 0)
                {
                    Addr = await _auth.GetOrRememberIP(addresses[0]);
                }
                else
                {
                    return BadRequest();
                }
            }
            var peer = await (await _peers.Open(Addr, port)).Query();
            return await ViewPeer(peer);
        }

        [HttpGet]
        public async Task<IActionResult> Peer(string address, int port = 443, string route = "")
        {
            var ip = await _auth.GetOrRememberIP(IPAddress.Parse(address));
            var peerClient = await _peers.Open(ip, port);
            var peer = await peerClient.Query();
            return Redirect($"/{peer.Address}-{peer.AddressPort}/{route}");
            // var vm = await peerClient.QueryViewModel(route);
            // return await ViewPeer(peer, vm);
        }

        [HttpGet]
        private async Task<IActionResult> ViewPeer(PeerModel peer, object proxiedViewModel = null)
        {
            ViewData["Controller"] = "Admin";
            ViewData["Action"] = nameof(Peer);

            return View(new PinkUmbrella.ViewModels.Peer.PeerViewModel() 
            {
                 MyProfile = await GetCurrentUserAsync(),
                 Peer = peer,
                 ProxiedViewModel = proxiedViewModel,
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TrustPeer(IPAddressModel Addr, int? port)
        {
            ViewData["Controller"] = "Admin";
            ViewData["Action"] = nameof(PreviewPeer);

            var peerClient = await _peers.Open(Addr, port);
            var peer = await peerClient.Query();
            if (peer != null)
            {
                await _peers.AddPeer(peer);
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Peer), new { address = Addr.Address, port = port });
                }
            }
            return View(new PinkUmbrella.ViewModels.Peer.PeerViewModel()
            {
                 MyProfile = await GetCurrentUserAsync(),
                 Peer = await (await _peers.Open(Addr, port)).Query(),
            });
        }

        [HttpGet]
        public async Task<IActionResult> ThrowException()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "admin"))
            {
                throw new Exception("You threw this exception");
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Posts()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "admin"))
            {
                ViewData["Controller"] = "Admin";
                ViewData["Action"] = nameof(Posts);
                return View(new PostsViewModel()
                {
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

        [HttpGet]
        public async Task<IActionResult> Community()
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "admin"))
            {
                ViewData["Controller"] = "Admin";
                ViewData["Action"] = nameof(Community);
                return View(new CommunityViewModel()
                {
                    MyProfile = user
                });
            }
            else
            {
                return Redirect("/Error/404");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GiveAccessToGroup(int toUserId, string group)
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "admin"))
            {
                if (await _roleManager.RoleExistsAsync(group))
                {
                    var code = await _userProfiles.NewGroupAccessCode(user.Id, toUserId, group);
                    return Content($"You have given {toUserId} access to {group}. The link is\n<a href=\"/AddMeToGroup/{code.Code}\">{code.Code}</a>");
                }
            }
            
            return Redirect("/Error/404");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(int postId, string group)
        {
            var user = await GetCurrentUserAsync();
            if (Debugger.IsAttached || await _userManager.IsInRoleAsync(user, "admin"))
            {
                int[] recipients = null;
                if (group == "*")
                {
                    recipients = _userManager.Users.Select(u => u.Id).ToArray();
                }
                else if (await _roleManager.RoleExistsAsync(group))
                {
                    recipients = (await _userManager.GetUsersInRoleAsync(group)).Select(u => u.Id).ToArray();
                }

                await _notifications.Publish(new Notification() {
                    FromUserId = user.Id,
                    Priority = NotificationPriority.Normal,
                    Type = NotificationType.DIRECT_NOTIFICATION,
                    Subject = ReactionSubject.Post,
                    SubjectId = postId,
                }, recipients);
                return Content($"You have sent {postId} to {group} ({recipients.Length} recipients). ");
            }
            
            return Redirect("/Error/404");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UntrustPeer(string address, int port)
        {
            ViewData["Controller"] = "Admin";
            ViewData["Action"] = nameof(UntrustPeer);

            var ip = await _auth.GetOrRememberIP(IPAddress.Parse(address));
            var peerClient = await _peers.Open(ip, port);
            var peer = await peerClient.Query();
            if (peer != null)
            {
                await _peers.AddPeer(peer);
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Peer), new { address, port });
                }
            }
            return View(new PinkUmbrella.ViewModels.Peer.PeerViewModel() {
                 MyProfile = await GetCurrentUserAsync(),
                 Peer = peer,
            });
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            ViewData["Controller"] = "Admin";
            ViewData["Action"] = nameof(Settings);
            return View(new SettingsViewModel()
            {
                MyProfile = await GetCurrentUserAsync(),
                Settings = _settings.Site,
            });
        }

        [HttpGet]
        public async Task<IActionResult> Toggles()
        {
            ViewData["Controller"] = "Admin";
            ViewData["Action"] = nameof(Toggles);
            return View(new FeatureFlagsViewModel()
            {
                MyProfile = await GetCurrentUserAsync(),
                FeatureFlags = await _settings.GetFeatureFlags(),
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSetting(string Key, string Value)
        {
            await _settings.Update(Key, Value);
            return NoContent();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateToggle(string Key, string Value)
        {
            await _settings.UpdateToggle(Key, Value);
            return NoContent();
        }
    }
}