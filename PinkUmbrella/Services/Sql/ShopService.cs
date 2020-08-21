using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Repositories;
using Estuary.Core;
using Estuary.Services;
using Estuary.Actors;
using Estuary.Util;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Sql
{
    public class ShopService : IShopService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPublicProfileService _users;
        private readonly ITagService _tags;
        private readonly IActivityStreamRepository _activityStreams;
        private readonly IObjectReferenceService _handles;

        public ShopService(SimpleDbContext dbContext, IPublicProfileService users, ITagService tags, IActivityStreamRepository activityStreams, IReactionService reactions, IObjectReferenceService handles)
        {
            _dbContext = dbContext;
            _users = users;
            _tags = tags;
            _activityStreams = activityStreams;
            _handles = handles;
        }

        public async Task<BaseObject> GetShopsTaggedUnder(BaseObject tag, int? viewerId)
        {
            // TODO: add support for external shops
            //var db = _dbContext.ShopTags;
            //var tagged = await (tag.objectId.HasValue ? db.Where(t => t.TagId == tag.objectId.Value) : throw new NotSupportedException("TagId only")).ToListAsync();
            var shops = new List<BaseObject>();
            // foreach (var t in tagged)
            // {
            //     var shop = await _activityStreams.Get(new ActivityStreamFilter("outbox")
            //     {
            //         objectId = t.ToId, viewerId = viewerId
            //     }.FixObjType(nameof(Common.Organization)));
            //     if (shop != null)
            //     {
            //         shops.Add(shop);
            //     }
            // }
            return shops.ToCollection();
        }

        public async Task<BaseObject> TryCreateShop(BaseObject shop)
        {
            if (shop is Common.Organization org)
            {
                await _dbContext.AddAsync(shop);
                await _dbContext.SaveChangesAsync();
            }
            return null;
        }

        public async Task<BaseObject> GetAllLocal() => await _activityStreams.Get(new ActivityStreamFilter("outbox")
                                                        { id = new PublicId(null, 0) }.FixObjType(nameof(Common.Organization)));
    }
}