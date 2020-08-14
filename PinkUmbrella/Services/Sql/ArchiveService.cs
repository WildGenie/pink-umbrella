using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Local;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;
using static Estuary.Objects.Common;
using Estuary.Objects;
using static Estuary.Activities.Common;

namespace PinkUmbrella.Services.Sql
{
    public class ArchiveService : IArchiveService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IUserProfileService _users;
        private readonly IPostService _posts;
        private readonly IWebHostEnvironment _env;
        private readonly ISettingsService _settings;

        public ArchiveService(SimpleDbContext dbContext,
            IUserProfileService users,
            IPostService posts,
            IWebHostEnvironment env,
            ISettingsService settings)
        {
            _dbContext = dbContext;
            _users = users;
            _posts = posts;
            _env = env;
            _settings = settings;

            var path = _env.ContentRootPath + "/Upload/Archive";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        public Task<BaseObject> DeleteMedia(int id, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<BaseObject> GetMedia(PublicId id, int? viewerId)
        {
            if (id.PeerId == 0)
            {
                return await Transform(await _dbContext.ArchivedMedia.FindAsync(id.Id));
            }
            return null;
        }

        public Task<BaseObject> Transform(ArchivedMediaModel archivedMediaModel)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetMedia(string path, int? viewerId) => GetMedia(new PublicId(BitConverter.ToInt32(Convert.FromBase64String(path), 0), 0), viewerId);

        public async Task<CollectionObject> GetMediaForUser(PublicId userId, int? viewerId, CommonMediaType? type, PaginationModel pagination)
        {
            if (userId.PeerId == 0)
            {
                var query = _dbContext.ArchivedMedia.Where(p => p.UserId == userId.Id);
                if (type.HasValue)
                {
                    query = query.Where(m => m.MediaType == type.Value);
                }
                var paginated = await query.ToListAsync(); // .OrderByDescending(p => p.WhenCreated)

                var keepers = new List<BaseObject>();
                foreach (var p in paginated) {
                    keepers.Add(await Transform(p));
                }
                return new OrderedCollectionPageObject {
                    items = keepers.Skip(pagination.start).Take(pagination.count).ToList(),
                    totalItems = keepers.Count(),
                    startIndex = pagination.start,
                };
            }
            else
            {
                return new CollectionObject();
            }
        }

        // public async Task<CollectionObject> GetMostBlockedMedia()
        // {
        //     var media = _dbContext.ArchivedMedia.Where(p => p.BlockCount > 0).OrderByDescending(p => p.BlockCount);
        //     return await ToPaginatedModel(media);
        // }

        // public async Task<CollectionObject> GetMostDislikedMedia()
        // {
        //     var media = _dbContext.ArchivedMedia.Where(p => p.DislikeCount > 0).OrderByDescending(p => p.DislikeCount);
        //     return await ToPaginatedModel(media);
        // }

        // public async Task<CollectionObject> GetMostReportedMedia()
        // {
        //     var media = _dbContext.ArchivedMedia.Where(p => p.ReportCount > 0).OrderByDescending(p => p.ReportCount);
        //     return await ToPaginatedModel(media);
        // }

        // private async Task<PaginatedModel<BaseObject>> ToPaginatedModel(IQueryable<BaseObject> media)
        // {
        //     return new PaginatedModel<BaseObject>()
        //     {
        //         Items = await media.Take(10).ToListAsync(),
        //         Total = media.Count(),
        //         Pagination = new PaginationModel(),
        //     };
        // }

        public Task<Stream> GetStream(BaseObject media, int? viewerId)
        {
            var actualPath = _env.ContentRootPath + "/Upload/Archive/" + media.objectId.Value;
            var fstream = new FileStream(actualPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream>(fstream);
        }

        // public async Task<string> GetLocalPath(string path, int? viewerId)
        // {
        //     var media = await _dbContext.ArchivedMedia.SingleOrDefaultAsync(m => m.Path == path);
        //     var actualPath = media != null ? _env.ContentRootPath + "/Upload/Archive/" + media.Path : null;
        //     return actualPath;
        // }

        public Task<MediaScanResultModel> ScanMediaForKKKops(BaseObject media, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<MediaScanResultModel> ScanMediaForProfanity(BaseObject media, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<MediaScanResultModel> ScanMediaForVirusOrBadThings(BaseObject media, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        // public async Task<NewMediaResult> TryUploadMedias(List<BaseObject> medias)
        // {
        //     foreach (var media in medias) {
        //         var error = await UploadMedia(media);
        //         if (error.Error) {
        //             return error;
        //         }
        //     }
            
        //     return new NewMediaResult() {
        //         Error = false,
        //         Medias = medias
        //     };
        // }

        public async Task<NewMediaResult> UploadMedia(BaseObject media)
        {
            if (media is Create createMedia)
            {
                if (createMedia.obj.attachment != null && createMedia.obj.attachment.items != null && createMedia.obj.attachment.items.Count > 0)
                {
                    foreach (var attch in createMedia.obj.attachment.items)
                    {
                        if (attch is Document doc)
                        {
                            var attchModel = new ArchivedMediaModel
                            {
                                MediaType = (CommonMediaType) Enum.Parse(typeof(CommonMediaType), doc.type),
                                SizeBytes = 0,
                                UserId = media.UserId.Value,
                            };
                            _dbContext.ArchivedMedia.Add(attchModel);
                            await _dbContext.SaveChangesAsync();

                            using (var newStream = File.Create(LocalPath(attchModel)))
                            {
                                using (var oldStream = new FileStream(doc.url.content, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    await oldStream.CopyToAsync(newStream);
                                }
                                File.Delete(doc.url.content);
                            }

                            attchModel.UploadedStatus = true;
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            media.published = DateTime.UtcNow;

            return new NewMediaResult() {
                Error = false,
                Medias = new List<BaseObject>() { media }
            };
        }

        public CommonMediaType? ResolveMediaType(string path)
        {
            switch (Path.GetExtension(path).ToLower())
            {
                case ".png": return CommonMediaType.Photo;
                case ".jpeg": return CommonMediaType.Photo;
                case ".jpg": return CommonMediaType.Photo;
                // case ".avi": return CommonMediaType.Video;
                // case ".mp4": return CommonMediaType.Video;
                // case ".webm": return CommonMediaType.Video;
                default: return null;
            }
        }

        public string LocalPath(BaseObject doc)
        {
            throw new NotImplementedException();
        }

        public string LocalPath(ArchivedMediaModel media) => _env.ContentRootPath + $"/Upload/Archive/{media.UserId}-{media.Id}";

        public async Task CopyUpload(ModelStateDictionary modelState, List<IFormFile> files, List<BaseObject> fileModels)
        {
            for (int i = 0; i < files.Count; i++)
            {
                var file = fileModels[i];
                var ifile = files[i];
                string finalPath = null;
                var localPath = LocalPath(file);
                try
                {
                    using (var stream = System.IO.File.Create(localPath))
                    {
                        await ifile.CopyToAsync(stream);
                    }
                    finalPath = Path.ChangeExtension(System.IO.Path.GetTempFileName(), Path.GetExtension(localPath));
                    using (var image = SixLabors.ImageSharp.Image.Load(localPath)) 
                    {
                        if (Math.Min(image.Width, image.Height) < _settings.Site.MinImageDimensionSize)
                        {
                            modelState.AddModelError("Files", $"Image {ifile.FileName} must be at least {_settings.Site.MinImageDimensionSize}x{_settings.Site.MinImageDimensionSize} pixels");
                            break;
                        }
                        image.Mutate(x => x.AutoOrient());
                        if (Math.Max(image.Width, image.Height) > _settings.Site.MaxImageDimensionSize)
                        {
                            if (image.Width > image.Height)
                            {
                                image.Mutate(x => x.Resize(_settings.Site.MaxImageDimensionSize, (int) Math.Round(image.Height * 1.0 * _settings.Site.MaxImageDimensionSize / image.Width)));
                            }
                            else
                            {
                                image.Mutate(x => x.Resize((int) Math.Round(image.Width * 1.0 * _settings.Site.MaxImageDimensionSize / image.Height), _settings.Site.MaxImageDimensionSize));
                            }
                        }

                        // Wipe EXIF data
                        image.Metadata.ExifProfile = null;
                        
                        image.Save(finalPath); 
                    }
                }
                catch (Exception e)
                {
                    if (System.IO.File.Exists(localPath))
                    {
                        System.IO.File.Delete(localPath);
                    }
                    
                    if (finalPath != null && System.IO.File.Exists(finalPath))
                    {
                        System.IO.File.Delete(finalPath);
                    }
                    modelState.AddModelError("Files", e, null);
                    break;
                }
                System.IO.File.Delete(localPath);
                // file.Path = finalPath;
            }

            if (modelState.ErrorCount == 0)
            {
                // var mediaResult = await UploadMedia(fileModels);
                // if (mediaResult.Error)
                // {
                //     modelState.AddModelError("Files", "Error while uploading files");
                // }
                // else
                // {
                //     // Ok(new { count = Files.Count, size, medias = mediaResult.Medias })
                // }
            }
        }

        public Task<List<BaseObject>> GenModels(ModelStateDictionary modelState, List<IFormFile> files, string description, string title, string attribution, int? relatedPostId, Visibility visibility)
        {
            return Task.FromResult(files.Select((f, i) => {
                if (f.Length == 0)
                {
                    modelState.AddModelError("Files", $"File {f.FileName} is empty");
                }
                else if (f.Length > _settings.Site.MaxFileSize)
                {
                    modelState.AddModelError("Files", $"File {f.FileName} too large ({f.Length} is over limit of {_settings.Site.MaxFileSize})");
                }
                else if (!System.IO.Path.HasExtension(f.FileName))
                {
                    modelState.AddModelError("Files", $"File missing extension: {f.FileName}");
                }
                else
                {
                    var mediaType = ResolveMediaType(f.FileName);
                    if (mediaType.HasValue)
                    {
                        Document doc = null;
                        switch (mediaType.Value)
                        {
                            case CommonMediaType.Audio: doc = new Audio(); break;
                            case CommonMediaType.Photo: doc = new Common.Image(); break;
                            case CommonMediaType.Video: doc = new Audio(); break;
                        }
                        doc.url = new Link { content = Path.ChangeExtension(System.IO.Path.GetTempFileName(), Path.GetExtension(f.FileName)) };
                        doc.summary = string.IsNullOrEmpty(description) ? f.FileName : description;
                        doc.name = string.IsNullOrWhiteSpace(title) ? f.FileName : title;
                        if (string.IsNullOrWhiteSpace(attribution))
                        {
                            doc.attributedTo.Add(new Note { content = attribution });
                        }
                        // doc.mediaType = mediaType.Value;
                        // doc.size = f.Length < Int32.MaxValue ? (int) f.Length : -1;

                        // OriginalName = f.FileName,
                        // SizeBytes = f.Length < Int32.MaxValue ? (int) f.Length : -1,
                        // RelatedPostId = RelatedPostId,
                        // UserId = user.UserId,
                        // Visibility = Visibility,
                        return doc;
                    }
                    else
                    {
                        modelState.AddModelError("Files", $"Unsupported file type: {f.FileName}");
                    }
                }
                return null;
            }).Cast<BaseObject>().ToList());
        }
    }
}