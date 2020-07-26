using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Actors;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;
using Tides.Services;
using Tides.Util;

namespace PinkUmbrella.Services.Sql
{
    public class ShopService : IShopService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPublicProfileService _users;
        private readonly ITagService _tags;
        private readonly IActivityStreamRepository _activityStreams;
        private readonly IReactionService _reactions;
        private readonly IHandleService _handles;

        public ShopService(SimpleDbContext dbContext, IPublicProfileService users, ITagService tags, IActivityStreamRepository activityStreams, IReactionService reactions, IHandleService handles)
        {
            _dbContext = dbContext;
            _users = users;
            _tags = tags;
            _activityStreams = activityStreams;
            _reactions = reactions;
            _handles = handles;
        }

        //public async Task DeleteShop(int id, int userId)
        //{
            // var shop = await _dbContext.Shops.FindAsync(id);
            // if (shop != null && shop.UserId == userId)
            // {
            //     var now = DateTime.UtcNow;
            //     shop.WhenDeleted = now;
            //     shop.LastUpdated = now;
            //     await _dbContext.SaveChangesAsync();
            // }
        //}

        // public async Task<CollectionObject> GetAllShops(int? viewerId)
        // {
        //     var ret = await _dbContext.Shops.OrderByDescending(s => s.WhenDeleted).Take(10).ToListAsync();
        //     var keepers = new List<BaseObject>();
        //     foreach (var shop in ret)
        //     {
        //         var obj = await _activityStreams.GetActors(new ActivityStreamFilter { objectId = shop.Id, userId = shop.UserId, types = new string[] { nameof(Common.Organization) } });
        //         var first = obj.items.FirstOrDefault();
        //         if (first != null)
        //         {
        //             keepers.Add(first);
        //         }
        //     }
        //     return keepers.ToCollection();
        // }

        // private async Task<CollectionObject> ToCollection(List<ReactionsSummaryModel> query)
        // {
        //     var shops = new List<BaseObject>();
        //     foreach (var summary in query.Take(10))
        //     {
        //         shops.Add(await GetShopById(new PublicId(summary.ToId, 0), null));
        //     }
        //     // var query = _dbContext.Shops.Where(s => s.DislikeCount > 0); query.OrderByDescending(s => s.DislikeCount).ToListAsync()
        //     return new CollectionObject {
        //         items = shops,
        //         totalItems = query.Count,
        //     };
        // }

        // public async Task<CollectionObject> GetMostDislikedShops()
        // {
        //     var query = await _reactions.GetMostDisliked(ReactionSubject.Shop, null);
        //     return await ToCollection(query);
        // }

        // public async Task<CollectionObject> GetMostReportedShops()
        // {
        //     var query = await _reactions.GetMostReported(ReactionSubject.Shop, null);
        //     return await ToCollection(query);
        // }

        // public async Task<BaseObject> GetShopByHandle(string handle, int? viewerId)
        // {
        //     var obj = await _activityStreams.GetActors(new ActivityStreamFilter { handle = handle, types = new string[] { nameof(Common.Organization) } });
        //     return obj.items.FirstOrDefault();
        // }

        public async Task<CollectionObject> GetShopsTaggedUnder(BaseObject tag, int? viewerId)
        {
            // TODO: add support for external shops
            var db = _dbContext.ShopTags;
            var tagged = await (tag.objectId.HasValue ? db.Where(t => t.TagId == tag.objectId.Value) : throw new NotSupportedException("TagId only")).ToListAsync();
            var shops = new List<BaseObject>();
            foreach (var t in tagged)
            {
                var shop = await _activityStreams.GetShops(new ActivityStreamFilter { objectId = t.ToId, viewerId = viewerId });
                if (shop != null)
                {
                    shops.Add(shop);
                }
            }
            return shops.ToCollection();
        }

        public async Task<ArgumentException> TryCreateShop(BaseObject shop)
        {
            if (shop is Common.Organization org)
            {
                if (await _handles.HandleExists(org.Handle))
                {
                    return new ArgumentException("Shop handle taken", nameof(org.Handle));
                }

                if (org.visibility == Visibility.VISIBLE_TO_FOLLOWERS)
                {
                    return new ArgumentException("Shop cannot be visible to followers only", nameof(shop.visibility));
                }
                
                await _dbContext.AddAsync(shop);
                await _dbContext.SaveChangesAsync();

                // var postInsertChanges = false;

                // if (shop.Tags.Any())
                // {
                //     await _tags.Save(ReactionSubject.Shop, shop.Tags, shop.UserId, shop.Id);
                //     postInsertChanges = true;
                // }

                // if (postInsertChanges)
                // {
                //     await _dbContext.SaveChangesAsync();
                // }
            }
            return null;
        }

        // public async Task<bool> HandleExists(string handle)
        // {
        //     //var shop = await _dbContext.Shops.FirstOrDefaultAsync(s => s.Handle.ToLower() == org.Handle.ToLower());
        //     var shop = await _dbContext.Shops.FirstOrDefaultAsync(u => u.Handle.ToLower() == handle);
        //     return shop != null;
        // }

        public async Task<CollectionObject> GetAllLocal() => await _activityStreams.GetShops(new ActivityStreamFilter { peerId = 0 });
    }
}