using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.ViewModels.Archive;
using Microsoft.AspNetCore.Http;
using PinkUmbrella.Repositories;
using Microsoft.AspNetCore.Authorization;
using PinkUmbrella.ViewModels;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Services.Local;
using Tides.Models;
using Tides.Models.Public;
using Tides.Objects;
using Tides.Services;

namespace PinkUmbrella.Controllers
{
    [FeatureGate(FeatureFlags.ControllerArchive)]
    public class ArchiveController : BaseController
    {
        private readonly ILogger<ArchiveController> _logger;
        private readonly IArchiveService _archive;
        private readonly StringRepository _strings;

        public ArchiveController(IWebHostEnvironment environment, ILogger<ArchiveController> logger, SignInManager<UserProfileModel> signInManager,
            UserManager<UserProfileModel> userManager, IPostService posts, IUserProfileService localProfiles, IPublicProfileService publicProfiles, IArchiveService archive,
            IReactionService reactions, StringRepository strings, ITagService tags, INotificationService notifications, IPeerService peers,
            IAuthService auth, ISettingsService settings, IActivityStreamRepository activityStreams):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings, activityStreams)
        {
            _logger = logger;
            _archive = archive;
            _strings = strings;
        }

        [Authorize]
        public async Task<IActionResult> Index(PaginationModel pagination)
        {
            ViewData["Controller"] = "Archive";
            ViewData["Action"] = nameof(Index);
            ViewData["Title"] = "Media Archive";
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user,
                Items = await _archive.GetMediaForUser(user.PublicId, user.UserId, null, pagination)
            };

            return View(model);
        }

        public async Task<IActionResult> Photos(PaginationModel pagination)
        {
            ViewData["Controller"] = "Archive";
            ViewData["Action"] = nameof(Photos);
            ViewData["Title"] = "Your Photos";
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user,
                Items = await _archive.GetMediaForUser(user.PublicId, user.UserId, CommonMediaType.Photo, pagination)
            };

            return View("Index", model);
        }

        public async Task<IActionResult> Videos(PaginationModel pagination)
        {
            ViewData["Controller"] = "Archive";
            ViewData["Action"] = nameof(Videos);
            ViewData["Title"] = "Your Videos";
            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel() {
                MyProfile = user,
                Items = await _archive.GetMediaForUser(user.PublicId, user.UserId, CommonMediaType.Video, pagination)
            };

            return View("Index", model);
        }

        [HttpGet("/Archive/Raw/{path}"), Route("/Archive/Raw/{path}")]
        public async Task<IActionResult> Raw(string path)
        {
            var user = await GetCurrentUserAsync();
            if (path.Contains('.'))
            {
                var pathSplit = path.Split('.');
                if (pathSplit.Length == 2)
                {
                    var media = await _archive.GetMedia(pathSplit[0], user?.UserId);
                    if (media != null)
                    {
                        var stream = await _archive.GetStream(media, user?.UserId);
                        var contentType = _strings.GetContentType(pathSplit[1]);
                        return new FileStreamResult(stream, contentType)
                        {
                            FileDownloadName = media.DisplayName + '.' + pathSplit[1]
                        };
                    }
                }
            }
            return NotFound();
        }

        [Route("/Archive/{id}")]
        public async Task<IActionResult> Media(string id)
        {
            ViewData["Controller"] = "Post";
            ViewData["Action"] = nameof(Media);
            var user = await GetCurrentUserAsync();
            if (id != null)
            {
                var media = await _archive.GetMedia(new PublicId(id), user?.UserId ?? -1);
                if (media != null)
                {
                    return View(new MediaViewModel() {
                        Media = media,
                        MyProfile = user
                    });
                }
            }
            
            return NotFound();
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Upload()
        {
            ViewData["Controller"] = "Archive";
            ViewData["Action"] = nameof(Upload);
            var user = await GetCurrentUserAsync();
            var model = new BaseViewModel() {
                MyProfile = user,
            };

            return View(model);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Upload(List<IFormFile> Files, string Description, string Title, string Attribution, int? RelatedPostId, Visibility Visibility)
        {
            long size = Files.Sum(f => f.Length);

            if (Files.Count == 0)
            {
                ModelState.AddModelError("Files", "No files selected");
            }
            else if (size < _settings.Site.MaxPayloadSize)
            {
                var user = await GetCurrentUserAsync();
                var fileModels = await _archive.GenModels(ModelState, Files, Description, Title, Attribution, RelatedPostId, Visibility);

                if (ModelState.ErrorCount == 0)
                {
                    await _archive.CopyUpload(ModelState, Files, fileModels);

                    if (ModelState.ErrorCount == 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Files", $"Total file upload is too large ({_settings.Site.MaxPayloadSize} max, total was {size})");
            }
            return View();
        }
    }
}
