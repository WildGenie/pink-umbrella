using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Local;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Sql
{
    public class ArchiveService : IArchiveService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IUserProfileService _users;
        private readonly IPostService _posts;
        private readonly IWebHostEnvironment _env;

        public ArchiveService(SimpleDbContext dbContext, IUserProfileService users, IPostService posts, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _users = users;
            _posts = posts;
            _env = env;

            var path = _env.ContentRootPath + "/Upload/Archive";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        public async Task BindReferences(ArchivedMediaModel media, int? viewerId)
        {
            if (media.RelatedPost == null && media.RelatedPostId.HasValue)
            {
                // TODO: give media peer ids
                media.RelatedPost = await _posts.GetPost(new PublicId(media.RelatedPostId.Value, 0), viewerId); 
            }

            if (media.User == null)
            {
                media.User = await _users.GetUser(media.UserId, viewerId);
            }

            media.ViewerId = viewerId;

            if (viewerId.HasValue)
            {
                media.Reactions = await _dbContext.PostReactions.Where(r => r.UserId == viewerId.Value && r.ToId == media.Id).ToListAsync();
                if (!media.ViewerIsPoster)
                {
                    var reactions = media.Reactions.Select(r => r.Type).ToHashSet();
                    media.HasLiked = reactions.Contains(ReactionType.Like);
                    media.HasDisliked = reactions.Contains(ReactionType.Dislike);
                    media.HasBlocked = reactions.Contains(ReactionType.Block);
                    media.HasReported = reactions.Contains(ReactionType.Report);

                    media.ViewerIsFollowing = (await _dbContext.ProfileReactions.FirstOrDefaultAsync(r => r.ToId == media.UserId && r.UserId == viewerId.Value && r.Type == ReactionType.Follow)) != null;
                
                    var blockOrReport = await _dbContext.ProfileReactions.FirstOrDefaultAsync(r => ((r.ToId == viewerId.Value && r.UserId == media.UserId) || (r.ToId == media.UserId && r.UserId == viewerId.Value) && (r.Type == ReactionType.Block || r.Type == ReactionType.Report)));
                    media.HasBeenBlockedOrReported =  blockOrReport != null;
                }
                else
                {
                    media.ViewerIsFollowing = true;
                }
            }
        }

        public bool CanView(ArchivedMediaModel media, int? viewerId)
        {
            if (media.ViewerIsPoster)
            {
                return true;
            }
            
            if (media.HasBeenBlockedOrReported)
            {
                return false;
            }

            switch (media.Visibility)
            {
                case Visibility.HIDDEN: return false;
                case Visibility.VISIBLE_TO_FOLLOWERS:
                if (!media.ViewerIsFollowing)
                {
                    return false;
                }
                break;
                case Visibility.VISIBLE_TO_REGISTERED:
                if (!viewerId.HasValue)
                {
                    return false;
                }
                break;
            }
            return true;
        }

        public Task<ArchivedMediaModel> DeleteMedia(int id, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ArchivedMediaModel> GetMedia(PublicId id, int? viewerId)
        {
            if (id.PeerId == 0)
            {
                var ret = await _dbContext.ArchivedMedia.FindAsync(id.Id);
                if (ret != null)
                {
                    await BindReferences(ret, viewerId);
                    if (CanView(ret, viewerId))
                    {
                        return ret;
                    }
                }
            }
            return null;
        }

        public Task<ArchivedMediaModel> GetMedia(string path, int? viewerId) => GetMedia(new PublicId(BitConverter.ToInt32(Convert.FromBase64String(path), 0), 0), viewerId);

        public async Task<PaginatedModel<ArchivedMediaModel>> GetMediaForUser(PublicId userId, int? viewerId, ArchivedMediaType? type, PaginationModel pagination)
        {
            if (userId.PeerId == 0)
            {
                var query = _dbContext.ArchivedMedia.Where(p => p.UserId == userId.Id);
                if (type.HasValue)
                {
                    query = query.Where(m => m.MediaType == type.Value);
                }
                var paginated = await query.OrderByDescending(p => p.WhenCreated).ToListAsync();

                var keepers = new List<ArchivedMediaModel>();
                foreach (var p in paginated) {
                    await BindReferences(p, viewerId);
                    if (CanView(p, viewerId))
                    {
                        keepers.Add(p);
                    }
                }
                return new PaginatedModel<ArchivedMediaModel>() {
                    Items = keepers.Skip(pagination.start).Take(pagination.count).ToList(),
                    Pagination = pagination,
                    Total = keepers.Count()
                };
            }
            else
            {
                return new PaginatedModel<ArchivedMediaModel>();
            }
        }

        public async Task<PaginatedModel<ArchivedMediaModel>> GetMostBlockedMedia()
        {
            var media = _dbContext.ArchivedMedia.Where(p => p.BlockCount > 0).OrderByDescending(p => p.BlockCount);
            return await ToPaginatedModel(media);
        }

        public async Task<PaginatedModel<ArchivedMediaModel>> GetMostDislikedMedia()
        {
            var media = _dbContext.ArchivedMedia.Where(p => p.DislikeCount > 0).OrderByDescending(p => p.DislikeCount);
            return await ToPaginatedModel(media);
        }

        public async Task<PaginatedModel<ArchivedMediaModel>> GetMostReportedMedia()
        {
            var media = _dbContext.ArchivedMedia.Where(p => p.ReportCount > 0).OrderByDescending(p => p.ReportCount);
            return await ToPaginatedModel(media);
        }

        private async Task<PaginatedModel<ArchivedMediaModel>> ToPaginatedModel(IQueryable<ArchivedMediaModel> media)
        {
            foreach (var m in media)
            {
                await BindReferences(m, null);
            }
            return new PaginatedModel<ArchivedMediaModel>()
            {
                Items = await media.Take(10).ToListAsync(),
                Total = media.Count(),
                Pagination = new PaginationModel(),
            };
        }

        public Task<Stream> GetStream(ArchivedMediaModel media, int? viewerId)
        {
            var actualPath = _env.ContentRootPath + "/Upload/Archive/" + media.Path;
            var fstream = new FileStream(actualPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream>(fstream);
        }

        public async Task<string> GetLocalPath(string path, int? viewerId)
        {
            var media = await _dbContext.ArchivedMedia.SingleOrDefaultAsync(m => m.Path == path);
            if (media != null)
            {
                await BindReferences(media, viewerId);
                if (CanView(media, viewerId))
                {
                    var actualPath = _env.ContentRootPath + "/Upload/Archive/" + media.Path;
                    return actualPath;
                }
            }
            
            return null;
        }

        public Task<MediaScanResultModel> ScanMediaForKKKops(ArchivedMediaModel media, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<MediaScanResultModel> ScanMediaForProfanity(ArchivedMediaModel media, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<MediaScanResultModel> ScanMediaForVirusOrBadThings(ArchivedMediaModel media, int? viewerId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<NewMediaResult> TryUploadMedias(List<ArchivedMediaModel> medias)
        {
            foreach (var media in medias) {
                var error = await TryCreateMedia(media);
                if (error.Error) {
                    return error;
                }
            }
            
            return new NewMediaResult() {
                Error = false,
                Medias = medias
            };
        }

        private async Task<NewMediaResult> TryCreateMedia(ArchivedMediaModel media)
        {
            media.WhenCreated = DateTime.UtcNow;
            _dbContext.ArchivedMedia.Add(media);
            await _dbContext.SaveChangesAsync();

            var oldPath = media.Path;
            media.Path = Convert.ToBase64String(BitConverter.GetBytes(media.Id)) + Path.GetExtension(oldPath);
            
            using (var newStream = File.Create(_env.ContentRootPath + "/Upload/Archive/" + media.Path))
            {
                using (var oldStream = new FileStream(oldPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await oldStream.CopyToAsync(newStream);
                }
                File.Delete(oldPath);
            }

            media.UploadedStatus = true;
            await _dbContext.SaveChangesAsync();

            return new NewMediaResult() {
                Error = false,
                Medias = new List<ArchivedMediaModel>() { media }
            };
        }

        public ArchivedMediaType? ResolveMediaType(string path)
        {
            switch (Path.GetExtension(path).ToLower())
            {
                case ".png": return ArchivedMediaType.Photo;
                case ".jpeg": return ArchivedMediaType.Photo;
                case ".jpg": return ArchivedMediaType.Photo;
                // case ".avi": return ArchivedMediaType.Video;
                // case ".mp4": return ArchivedMediaType.Video;
                // case ".webm": return ArchivedMediaType.Video;
                default: return null;
            }
        }

        public async Task UpdateShadowBanStatus(PublicId id, bool status)
        {
            if (id.PeerId == 0)
            {
                var media = await _dbContext.ArchivedMedia.FindAsync(id);
                media.ShadowBanned = status;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<ArchivedMediaModel>> GetAllLocal()
        {
            var all = await _dbContext.ArchivedMedia.ToListAsync();
            foreach (var item in all)
            {
                await BindReferences(item, null);
            }
            return all;
        }
    }
}