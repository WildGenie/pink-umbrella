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
using PinkUmbrella.ViewModels.Archive;
using Microsoft.AspNetCore.Http;
using PinkUmbrella.Repositories;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using PinkUmbrella.ViewModels;
using Microsoft.FeatureManagement.Mvc;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Services.Local;
using PinkUmbrella.Models.Public;
using Poncho.Models;
using Poncho.Models.Public;

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
            IAuthService auth, ISettingsService settings):
            base(environment, signInManager, userManager, posts, localProfiles, publicProfiles, reactions, tags, notifications, peers, auth, settings)
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
                Items = await _archive.GetMediaForUser(user.PublicId, user.UserId, ArchivedMediaType.Photo, pagination)
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
                Items = await _archive.GetMediaForUser(user.PublicId, user.UserId, ArchivedMediaType.Video, pagination)
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
            const int MaxFileSize = 4000000;
            const int MaxPayloadSize = 4000000 * 5;
            const int MaxImageDimensionSize = 1920;
            const int MinImageDimensionSize = 480;
            long size = Files.Sum(f => f.Length);

            if (Files.Count == 0)
            {
                ModelState.AddModelError("Files", "No files selected");
            }
            else if (size < MaxPayloadSize)
            {
                var user = await GetCurrentUserAsync();
                var fileModels = Files.Select((f, i) => {
                    if (f.Length == 0)
                    {
                        ModelState.AddModelError("Files", $"File {f.FileName} is empty");
                    }
                    else if (f.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("Files", $"File {f.FileName} too large ({f.Length} is over limit of {MaxFileSize})");
                    }
                    else if (!System.IO.Path.HasExtension(f.FileName))
                    {
                        ModelState.AddModelError("Files", $"File missing extension: {f.FileName}");
                    }
                    else
                    {
                        var mediaType = _archive.ResolveMediaType(f.FileName);
                        if (mediaType.HasValue)
                        {
                            return new ArchivedMediaModel() {
                                Path = Path.ChangeExtension(System.IO.Path.GetTempFileName(), Path.GetExtension(f.FileName)),
                                Description = string.IsNullOrEmpty(Description) ? f.FileName : Description,
                                Attribution = Attribution,
                                OriginalName = f.FileName,
                                SizeBytes = f.Length < Int32.MaxValue ? (int) f.Length : -1,
                                DisplayName = string.IsNullOrWhiteSpace(Title) ? f.FileName : Title,
                                RelatedPostId = RelatedPostId,
                                UserId = user.UserId,
                                Visibility = Visibility,
                                MediaType = mediaType.Value,
                            };
                        }
                        else
                        {
                            ModelState.AddModelError("Files", $"Unsupported file type: {f.FileName}");
                        }
                    }
                    return null;
                }).ToList();

                if (ModelState.ErrorCount == 0)
                {
                    for (int i = 0; i < Files.Count; i++)
                    {
                        var file = fileModels[i];
                        var ifile = Files[i];
                        string finalPath = null;
                        try
                        {
                            using (var stream = System.IO.File.Create(file.Path))
                            {
                                await ifile.CopyToAsync(stream);
                            }
                            finalPath = Path.ChangeExtension(System.IO.Path.GetTempFileName(), Path.GetExtension(file.Path));
                            using (Image image = Image.Load(file.Path)) 
                            {
                                if (Math.Min(image.Width, image.Height) < MinImageDimensionSize)
                                {
                                    ModelState.AddModelError("Files", $"Image {ifile.FileName} must be at least {MinImageDimensionSize}x{MinImageDimensionSize} pixels");
                                    break;
                                }
                                image.Mutate(x => x.AutoOrient());
                                if (Math.Max(image.Width, image.Height) > MaxImageDimensionSize)
                                {
                                    if (image.Width > image.Height)
                                    {
                                        image.Mutate(x => x.Resize(MaxImageDimensionSize, (int) Math.Round(image.Height * 1.0 * MaxImageDimensionSize / image.Width)));
                                    }
                                    else
                                    {
                                        image.Mutate(x => x.Resize((int) Math.Round(image.Width * 1.0 * MaxImageDimensionSize / image.Height), MaxImageDimensionSize));
                                    }
                                }

                                // Wipe EXIF data
                                image.Metadata.ExifProfile = null;
                                
                                image.Save(finalPath); 
                            }
                        }
                        catch (Exception e)
                        {
                            if (System.IO.File.Exists(file.Path))
                            {
                                System.IO.File.Delete(file.Path);
                            }
                            
                            if (finalPath != null && System.IO.File.Exists(finalPath))
                            {
                                System.IO.File.Delete(finalPath);
                            }
                            ModelState.AddModelError("Files", e, null);
                            break;
                        }
                        System.IO.File.Delete(file.Path);
                        file.Path = finalPath;
                    }

                    if (ModelState.ErrorCount == 0)
                    {
                        var mediaResult = await _archive.TryUploadMedias(fileModels);
                        if (mediaResult.Error)
                        {
                            ModelState.AddModelError("Files", "Error while uploading files");
                        }
                        else
                        {
                            return RedirectToAction(nameof(Index));
                            // Ok(new { count = Files.Count, size, medias = mediaResult.Medias })
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Files", $"Total file upload is too large ({MaxPayloadSize} max, total was {size})");
            }
            return View();
        }
    }
}
