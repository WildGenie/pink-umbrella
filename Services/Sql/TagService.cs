using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Util;

namespace PinkUmbrella.Services.Sql
{
    public class TagService : ITagService
    {
        private readonly SimpleDbContext _dbContext;

        public TagService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BindReferences(TaggedModel tag, int? viewerId)
        {
            if (tag.Tag == null)
            {
                tag.Tag = await _dbContext.AllTags.FindAsync(tag.TagId);
                await BindReferences(tag.Tag);
            }
        }

        public async Task BindReferences(TagModel tag)
        {
            switch (tag.Tag)
            {
                default: tag.Topic = "This is an interesting topic!"; break;
            }
        }

        public bool CanView(TaggedModel tag, int? viewerId)
        {
            return true;
        }

        public async Task<PaginatedModel<TagModel>> GetMostBlockedTags()
        {
            var query = _dbContext.AllTags.Where(t => t.BlockCount > 0);
            return await Paginate(query, query.OrderByDescending(t => t.BlockCount));
        }

        public async Task<PaginatedModel<TagModel>> GetMostDislikedTags()
        {
            var query = _dbContext.AllTags.Where(t => t.DislikeCount > 0);
            return await Paginate(query, query.OrderByDescending(t => t.DislikeCount));
        }

        public async Task<PaginatedModel<TagModel>> GetMostLikedTags()
        {
            var query = _dbContext.AllTags.Where(t => t.LikeCount > 0);
            return await Paginate(query, query.OrderByDescending(t => t.LikeCount));
        }

        private static async Task<PaginatedModel<T>> Paginate<T>(IQueryable<T> query, IQueryable<T> queryOrdered)
        {
            return new PaginatedModel<T>()
            {
                Items = await queryOrdered.Take(10).ToListAsync(),
                Total = await query.CountAsync(),
                Pagination = new PaginationModel(),
            };
        }

        public async Task<PaginatedModel<UsedTagModel>> GetMostUsedTags()
        {
            var queryStr = $"SELECT b.* FROM (" +
                                "SELECT TagId 'Id', COUNT(TagId) 'UseCount' FROM (" +
                                    "SELECT TagId FROM PostTags CONCAT " +
                                    "SELECT TagId FROM ProfileTags CONCAT " +
                                    "SELECT TagId FROM ShopTags CONCAT " +
                                    "SELECT TagId FROM ArchivedMediaTags" +
                                ") GROUP BY TagId" +
                            ") b ORDER BY b.UseCount";
            return await QueryTagsFromIds(queryStr);
        }

        private async Task<PaginatedModel<UsedTagModel>> QueryTagsFromIds(string queryStr)
        {
            var tagCounts = new Dictionary<int, long>();
            var query = _dbContext.RawSqlQuery(queryStr, r => new { Id = (int)r[0], UseCount = (long)r[1] });
            var counts = query.OrderByDescending(c => c.UseCount).Take(10).ToList();
            var keepers = new List<UsedTagModel>();
            foreach (var ct in counts)
            {
                keepers.Add(new UsedTagModel()
                {
                    Tag = await GetTag(ct.Id, null),
                    UseCount = ct.UseCount,
                });
            }
            return new PaginatedModel<UsedTagModel>()
            {
                Items = keepers,
                Total = query.Count,
                Pagination = new PaginationModel(),
            };
        }

        public async Task<TagModel> GetTag(string text, int? viewerId) => await _dbContext.AllTags.SingleOrDefaultAsync(t => t.Tag.ToLower() == text.ToLower());

        public async Task<TagModel> GetTag(int id, int? viewerId) => await _dbContext.AllTags.FindAsync(id);

        public async Task<List<TagModel>> GetTagsFor(int toId, ReactionSubject subject, int? viewerId)
        {
            var taggeds = new List<TaggedModel>();
            switch (subject)
            {
                case ReactionSubject.ArchivedMedia:
                    taggeds = await _dbContext.ArchivedMediaTags.Where(t => t.ToId == toId).ToListAsync();
                    break;
                case ReactionSubject.Post:
                    taggeds = await _dbContext.PostTags.Where(t => t.ToId == toId).ToListAsync();
                    break;
                case ReactionSubject.Profile:
                    taggeds = await _dbContext.ProfileTags.Where(t => t.ToId == toId).ToListAsync();
                    break;
                case ReactionSubject.Shop:
                    taggeds = await _dbContext.ShopTags.Where(t => t.ToId == toId).ToListAsync();
                    break;
                default:
                    break;
            }
            return await BindReferencesAndGetViewable(viewerId, taggeds);
        }

        private async Task<List<TagModel>> BindReferencesAndGetViewable(int? viewerId, List<TaggedModel> tags)
        {
            var keepers = new List<TagModel>();
            foreach (var t in tags)
            {
                await BindReferences(t, viewerId);
                if (CanView(t, viewerId))
                {
                    var tag = await this.GetTag(t.TagId, viewerId);
                    if (tag != null)
                    {
                        keepers.Add(tag);
                    }
                }
            }

            return keepers;
        }

        public async Task<List<TagModel>> GetTagsForSubject(ReactionSubject subject, int? viewerId)
        {
            var tags = new List<TaggedModel>();
            switch (subject)
            {
                case ReactionSubject.ArchivedMedia:
                    tags = await _dbContext.ArchivedMediaTags.ToListAsync();
                    break;
                case ReactionSubject.Post:
                    tags = await _dbContext.PostTags.ToListAsync();
                    break;
                case ReactionSubject.Profile:
                    tags = await _dbContext.ProfileTags.ToListAsync();
                    break;
                case ReactionSubject.Shop:
                    tags = await _dbContext.ShopTags.ToListAsync();
                    break;
                default:
                    break;
            }

            return await BindReferencesAndGetViewable(viewerId, tags);
        }

        public async Task<TagModel> TryGetOrCreateTag(TagModel tag, int? viewerId)
        {
            TagModel by_tag = null;
            if (tag.Id > 0)
            {
                by_tag = await GetTag(tag.Id, viewerId);
                if (by_tag == null)
                {
                    return null;
                }
            }
            else if (!string.IsNullOrWhiteSpace(tag.Tag))
            {
                by_tag = await GetTag(tag.Tag, viewerId);
            }
            else
            {
                return null;
            }

            if (by_tag != null)
            {
                return by_tag;
            }
            else if (viewerId.HasValue)
            {
                tag.Id = 0;
                tag.CreatedByUserId = viewerId.Value;
                await _dbContext.AllTags.AddAsync(tag);
                await _dbContext.SaveChangesAsync();
                return tag;
            }
            else
            {
                return null;
            }
        }

        public async Task<PaginatedModel<UsedTagModel>> GetMostUsedTagsForSubject(ReactionSubject subject)
        {
            var tags = await GetTagsForSubject(subject, null);
            var used = tags.GroupBy(g => g.Id).Select(g => new UsedTagModel() { Tag = g.First(), UseCount = g.Count() });
            return new PaginatedModel<UsedTagModel>() {
                Items = used.OrderByDescending(t => t.UseCount).Take(10).ToList(),
                Total = used.Count(),
                Pagination = new PaginationModel(),
            };
            // var queryStr = "SELECT b.* FROM (" +
            //                     $"SELECT TagId 'Id', COUNT(TagId) 'UseCount' FROM {subject}Tags " +
            //                     "GROUP BY TagId" +
            //                 ") b ORDER BY b.UseCount";
            // return await QueryTagsFromIds(queryStr);

        }

        public Task<PaginatedModel<TagModel>> GetMostBlockedTagsForSubject(ReactionSubject subject)
        {
            throw new System.NotImplementedException();
        }

        public Task<PaginatedModel<TagModel>> GetMostLikedTagsForSubject(ReactionSubject subject)
        {
            throw new System.NotImplementedException();
        }

        public Task<PaginatedModel<TagModel>> GetMostDislikedTagsForSubject(ReactionSubject subject)
        {
            throw new System.NotImplementedException();
        }
        
        public async Task<List<TagModel>> GetCompletionsForTag(string prefix)
        {
            prefix = prefix.ToLower();
            var ret = await _dbContext.AllTags.Where(t => t.Tag.ToLower().StartsWith(prefix)).Take(10).ToListAsync();
            if (ret.Count == 0 && Debugger.IsAttached)
            {
                ret = new List<TagModel>();
                for (int i = 0; i < 5; i++)
                {
                    ret.Add(new TagModel() { Id = -1, Tag = $"TestTag{i}" });
                }
                return ret;
            }
            
            return ret;
        }

        public async Task Save(ReactionSubject subject, List<TagModel> tags, int userId, int toId)
        {
            DbSet<TaggedModel> src = null;
            switch (subject)
            {
                case ReactionSubject.ArchivedMedia:
                    src = _dbContext.ArchivedMediaTags;
                    break;
                case ReactionSubject.Post:
                    src = _dbContext.PostTags;
                    break;
                case ReactionSubject.Profile:
                    src = _dbContext.ProfileTags;
                    break;
                case ReactionSubject.Shop:
                    src = _dbContext.ShopTags;
                    break;
                default:
                    break;
            }

            var alreadyTaggedIds = await src.Where(t => t.ToId == toId && t.UserId == userId).ToDictionaryAsync(k => k.TagId, v => v);
            var taggeds = tags.Where(t => !alreadyTaggedIds.ContainsKey(t.Id)).Select(t => new TaggedModel() { TagId = t.Id, ToId = toId, UserId = userId, WhenCreated = DateTime.UtcNow }).ToList();
            var noLongerTagged = new List<TaggedModel>();
            foreach (var noLongerTaggedId in alreadyTaggedIds.Keys.Except(tags.Select(t => t.Id)))
            {
                noLongerTagged.Add(alreadyTaggedIds[noLongerTaggedId]);
            }

            if (taggeds.Count > 0)
            {
                await src.AddRangeAsync(taggeds);
            }
            if (noLongerTagged.Count > 0)
            {
                src.RemoveRange(noLongerTagged);
            }
        }
    }
}