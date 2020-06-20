using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql
{
    public class ShopService : IShopService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IUserProfileService _users;
        private readonly ITagService _tags;

        public ShopService(SimpleDbContext dbContext, IUserProfileService users, ITagService tags)
        {
            _dbContext = dbContext;
            _users = users;
            _tags = tags;
        }

        public async Task BindReferences(ShopModel shop, int? viewerId)
        {
            if (shop.OwnerUser == null)
            {
                shop.OwnerUser = await _users.GetUser(shop.UserId, viewerId);
            }
            
            shop.ViewerId = viewerId;

            if (viewerId.HasValue)
            {
                shop.Reactions = await _dbContext.ShopReactions.Where(r => r.UserId == viewerId.Value && r.ToId == shop.Id).ToListAsync();
                if (!shop.ViewerIsOwner)
                {
                    var reactions = shop.Reactions.Select(r => r.Type).ToHashSet();
                    shop.HasLiked = reactions.Contains(ReactionType.Like);
                    shop.HasDisliked = reactions.Contains(ReactionType.Dislike);
                    shop.HasReported = reactions.Contains(ReactionType.Report);
                
                    var blockOrReport = await _dbContext.ProfileReactions.FirstOrDefaultAsync(r => ((r.ToId == viewerId.Value && r.UserId == shop.UserId) || (r.ToId == shop.UserId && r.UserId == viewerId.Value) && (r.Type == ReactionType.Block || r.Type == ReactionType.Report)));
                    shop.HasBeenBlockedOrReported =  blockOrReport != null; // p.Reactions.Any(r => r.Type == ReactionType.Block || r.Type == ReactionType.Report)
                }
            }

            if (shop.Tags == null)
            {
                shop.Tags = new List<TagModel>();
            }
            else if (shop.Tags.Count == 0)
            {
                shop.Tags = await _tags.GetTagsFor(shop.Id, ReactionSubject.Shop, viewerId);
            }
        }

        public bool CanView(ShopModel shop, int? viewerId)
        {
            if (shop.ViewerIsOwner)
            {
                return true;
            }
            
            if (shop.HasBeenBlockedOrReported)
            {
                return false;
            }

            switch (shop.Visibility)
            {
                case Visibility.HIDDEN: return false;
                case Visibility.VISIBLE_TO_REGISTERED:
                if (!viewerId.HasValue)
                {
                    return false;
                }
                break;
            }
            return true;
        }

        public async Task DeleteShop(int id, int userId)
        {
            var shop = await _dbContext.Shops.FindAsync(id);
            if (shop != null && shop.UserId == userId)
            {
                var now = DateTime.UtcNow;
                shop.WhenDeleted = now;
                shop.LastUpdated = now;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<ShopModel>> GetAllShops(int? viewerId)
        {
            var ret = await _dbContext.Shops.OrderByDescending(s => s.WhenDeleted).Take(10).ToListAsync();
            var keepers = new List<ShopModel>();
            foreach (var shop in ret)
            {
                await BindReferences(shop, viewerId);
                if (CanView(shop, viewerId))
                {
                    keepers.Add(shop);
                }
            }
            return keepers;
        }

        public async Task<PaginatedModel<ShopModel>> GetMostDislikedShops()
        {
            var query = _dbContext.Shops.Where(s => s.DislikeCount > 0);
            return new PaginatedModel<ShopModel>() {
                Items = await query.OrderByDescending(s => s.DislikeCount).Take(10).ToListAsync(),
                Total = await query.CountAsync(),
                Pagination = new PaginationModel()
            };
        }

        public async Task<PaginatedModel<ShopModel>> GetMostReportedShops()
        {
            var query = _dbContext.Shops.Where(s => s.ReportCount > 0);
            return new PaginatedModel<ShopModel>() {
                Items = await query.OrderByDescending(s => s.ReportCount).Take(10).ToListAsync(),
                Total = await query.CountAsync(),
                Pagination = new PaginationModel()
            };
        }

        public async Task<ShopModel> GetShopByHandle(string handle, int? viewerId)
        {
            var ret = await _dbContext.Shops.SingleOrDefaultAsync(s => s.Handle == handle);
            if (ret != null)
            {
                await BindReferences(ret, viewerId);
                if (CanView(ret, viewerId))
                {
                    return ret;
                }
            }
            return null;
        }

        public async Task<ShopModel> GetShopById(int id, int? viewerId)
        {
            var ret = await _dbContext.Shops.FindAsync(id);
            if (ret != null)
            {
                await BindReferences(ret, viewerId);
                if (CanView(ret, viewerId))
                {
                    return ret;
                }
            }
            return null;
        }

        public async Task<List<ShopModel>> GetShopsForUser(int userId, int? viewerId)
        {
            var items = await _dbContext.Shops.Where(s => s.UserId == userId).ToListAsync();
            var keepers = new List<ShopModel>();
            foreach (var ret in items)
            {
                await BindReferences(ret, viewerId);
                if (CanView(ret, viewerId))
                {
                    keepers.Add(ret);
                }
            }
            return keepers;
        }

        public async Task<List<ShopModel>> GetShopsTaggedUnder(TagModel tag, int? viewerId)
        {
            var tagged = await _dbContext.ShopTags.Where(t => t.TagId == tag.Id).ToListAsync();
            var shops = new List<ShopModel>();
            foreach (var t in tagged)
            {
                var shop = await GetShopById(t.ToId, viewerId);
                if (shop != null)
                {
                    shops.Add(shop);
                }
            }
            return shops;
        }

        public async Task<ArgumentException> TryCreateShop(ShopModel shop)
        {
            if (shop.Visibility == Visibility.VISIBLE_TO_FOLLOWERS)
            {
                return new ArgumentException("Shop cannot be visible to followers only", nameof(shop.Visibility));
            }

            var existingShopHandle = await _dbContext.Shops.FirstOrDefaultAsync(s => s.Handle.ToLower() == shop.Handle.ToLower());
            if (existingShopHandle != null)
            {
                return new ArgumentException("Shop handle taken", nameof(shop.Handle));
            }

            shop.WhenCreated = DateTime.UtcNow;
            shop.LastUpdated = DateTime.UtcNow;
            await _dbContext.AddAsync(shop);
            await _dbContext.SaveChangesAsync();

            var postInsertChanges = false;

            if (shop.Tags.Any())
            {
                await _tags.Save(ReactionSubject.Post, shop.Tags, shop.UserId, shop.Id);
                postInsertChanges = true;
            }

            if (postInsertChanges)
            {
                await _dbContext.SaveChangesAsync();
            }
            return null;
        }
    }
}