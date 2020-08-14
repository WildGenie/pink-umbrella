using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Util;
using Estuary.Core;
using Tides.Models;
using Estuary.Util;
using StackExchange.Redis;
using Tides.Models.Public;
using System;
using static Estuary.Objects.Common;

namespace PinkUmbrella.Services.Sql
{
    public class TagService : ITagService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly ConnectionMultiplexer _redis;

        public TagService(SimpleDbContext dbContext, ConnectionMultiplexer redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        // public async Task BindReferences(TaggedModel tag, int? viewerId)
        // {
        //     if (tag.Tag == null)
        //     {
        //         tag.Tag = await _dbContext.AllTags.FindAsync(tag.TagId);
        //         await BindReferences(tag.Tag);
        //     }
        // }

        public async Task BindReferences(TagModel tag)
        {
            switch (tag.Tag)
            {
                default: tag.Topic = "This is an interesting topic!"; break;
            }
            if (tag.Id > 0)
            {
                // var counts = await Task.WhenAll(
                //     _dbContext.PostTags.CountAsync(t => t.TagId == tag.Id),
                //     _dbContext.ArchivedMediaTags.CountAsync(t => t.TagId == tag.Id),
                //     _dbContext.ProfileTags.CountAsync(t => t.TagId == tag.Id),
                //     _dbContext.ShopTags.CountAsync(t => t.TagId == tag.Id));
                // tag.UseCount = counts.Sum();
            }
        }

        public async Task<CollectionObject> GetMostBlockedTags()
        {
            // var query = _dbContext.AllTags.Where(t => t.BlockCount > 0);
            // return await Paginate(query, query.OrderByDescending(t => t.BlockCount));
            await Task.Delay(1);
            return null;
        }

        public async Task<CollectionObject> GetMostDislikedTags()
        {
            // var query = _dbContext.AllTags.Where(t => t.DislikeCount > 0);
            // return await Paginate(query, query.OrderByDescending(t => t.DislikeCount));
            await Task.Delay(1);
            return null;
        }

        public async Task<CollectionObject> GetMostLikedTags()
        {
            // var query = _dbContext.AllTags.Where(t => t.LikeCount > 0);
            // return await Paginate(query, query.OrderByDescending(t => t.LikeCount));
            await Task.Delay(1);
            return null;
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

        public async Task<CollectionObject> GetMostUsedTags()
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

        private async Task<CollectionObject> QueryTagsFromIds(string queryStr)
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
            return new CollectionObject()
            {
                items = (await Task.WhenAll(keepers.Select(t => Transform(t)))).ToList(),
                totalItems = query.Count,
            };
        }

        public Task<BaseObject> Transform(UsedTagModel tag)
        {
            return null;
        }

        public Task<BaseObject> Transform(TagModel tag)
        {
            return Task.FromResult<BaseObject>(tag != null ? new Note { content = tag.Tag, objectId = tag.Id } : null);
        }

        public async Task<BaseObject> GetTag(string text, int? viewerId)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            return await Transform(await _dbContext.AllTags.SingleOrDefaultAsync(t => t.Tag.ToLower() == text.ToLower()));
        }

        public async Task<BaseObject> GetTag(int id, int? viewerId) => await Transform(await _dbContext.AllTags.FindAsync(id));

        // private async Task<CollectionObject> BindReferencesAndGetViewable(int? viewerId, List<TaggedModel> tags)
        // {
        //     var keepers = new List<TagModel>();
        //     foreach (var t in tags)
        //     {
        //         await BindReferences(t, viewerId);
        //         if (CanView(t, viewerId))
        //         {
        //             var tag = await this.GetTag(t.TagId, viewerId);
        //             if (tag != null)
        //             {
        //                 keepers.Add(tag);
        //             }
        //         }
        //     }

        //     return keepers;
        // }

        public Task<CollectionObject> GetTagsForSubject(string subject, int? viewerId)
        {
            return Task.FromResult<CollectionObject>(null);
            // var tags = new List<TaggedModel>();
            // switch (subject)
            // {
            //     case ReactionSubject.ArchivedMedia:
            //         tags = await _dbContext.ArchivedMediaTags.ToListAsync();
            //         break;
            //     case ReactionSubject.Post:
            //         tags = await _dbContext.PostTags.ToListAsync();
            //         break;
            //     case ReactionSubject.Profile:
            //         tags = await _dbContext.ProfileTags.ToListAsync();
            //         break;
            //     case ReactionSubject.Shop:
            //         tags = await _dbContext.ShopTags.ToListAsync();
            //         break;
            //     default:
            //         break;
            // }

            // return (await Task.WhenAll(tags.Select(Transform))).ToCollection();
        }

        public async Task<BaseObject> TryGetOrCreateTag(BaseObject tag, int? viewerId)
        {
            BaseObject by_tag = null;
            if (tag.objectId.HasValue && tag.objectId.Value > 0)
            {
                by_tag = await GetTag(tag.objectId.Value, viewerId);
                if (by_tag == null)
                {
                    return null;
                }
            }
            else if (!string.IsNullOrWhiteSpace(tag.content))
            {
                by_tag = await GetTag(tag.content, viewerId);
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
                var tm = new TagModel
                {
                    Tag = tag.content,
                    CreatedByUserId = viewerId.Value
                };
                await _dbContext.AllTags.AddAsync(tm);
                await _dbContext.SaveChangesAsync();

                tag.objectId = tm.Id;
                return tag;
            }
            else
            {
                return null;
            }
        }

        public async Task<CollectionObject> GetMostUsedTagsForSubject(string subject)
        {
            var tags = await GetTagsForSubject(subject, null);
            var used = tags.items.GroupBy(g => g.objectId.Value).Select(g => new UsedTagModel() { Tag = g.First(), UseCount = g.Count() });
            return new CollectionObject() {
                items = (await Task.WhenAll(used.OrderByDescending(t => t.UseCount).Take(10).Select(Transform))).ToList(),
                totalItems = used.Count(),
            };
            // var queryStr = "SELECT b.* FROM (" +
            //                     $"SELECT TagId 'Id', COUNT(TagId) 'UseCount' FROM {subject}Tags " +
            //                     "GROUP BY TagId" +
            //                 ") b ORDER BY b.UseCount";
            // return await QueryTagsFromIds(queryStr);

        }

        public Task<CollectionObject> GetMostBlockedTagsForSubject(string subject)
        {
            throw new System.NotImplementedException();
        }

        public Task<CollectionObject> GetMostLikedTagsForSubject(string subject)
        {
            throw new System.NotImplementedException();
        }

        public Task<CollectionObject> GetMostDislikedTagsForSubject(string subject)
        {
            throw new System.NotImplementedException();
        }
        
        public async Task<CollectionObject> GetCompletionsForTag(string prefix)
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
            }
            
            return (await Task.WhenAll(ret.Select(Transform))).ToCollection();
        }

        public async Task Save(List<TagModel> tags, int userId, PublicId toId)
        {
            //var val = await _redis.GetDatabase().StringGetAsync(new RedisKey[] { "", "" });
            //return //val.FirstOrDefault().IsInteger ? int.Parse(val.FirstOrDefault().ToString()) : 0;
            // DbSet<TaggedModel> src = null;
            // switch (subject)
            // {
            //     case ReactionSubject.ArchivedMedia:
            //         src = _dbContext.ArchivedMediaTags;
            //         break;
            //     case ReactionSubject.Post:
            //         src = _dbContext.PostTags;
            //         break;
            //     case ReactionSubject.Profile:
            //         src = _dbContext.ProfileTags;
            //         break;
            //     case ReactionSubject.Shop:
            //         src = _dbContext.ShopTags;
            //         break;
            //     default:
            //         break;
            // }

            // var alreadyTaggedIds = await src.Where(t => t.ToId == toId && t.UserId == userId).ToDictionaryAsync(k => k.TagId, v => v);
            // var taggeds = tags.Where(t => !alreadyTaggedIds.ContainsKey(t.Id)).Select(t => new TaggedModel() { TagId = t.Id, ToId = toId, UserId = userId, WhenCreated = DateTime.UtcNow }).ToList();
            // var noLongerTagged = new List<TaggedModel>();
            // foreach (var noLongerTaggedId in alreadyTaggedIds.Keys.Except(tags.Select(t => t.Id)))
            // {
            //     noLongerTagged.Add(alreadyTaggedIds[noLongerTaggedId]);
            // }

            // if (taggeds.Count > 0)
            // {
            //     await src.AddRangeAsync(taggeds);
            // }
            // if (noLongerTagged.Count > 0)
            // {
            //     src.RemoveRange(noLongerTagged);
            // }
        }
    }
}