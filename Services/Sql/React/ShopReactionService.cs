using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql.React
{
    public class ShopReactionService : IReactableService
    {
        private readonly SimpleDbContext _dbContext;

        public ShopReactionService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string ControllerName => "Shop";

        public ReactionSubject Subject => ReactionSubject.Shop;

        public Task<List<int>> GetIds()
        {
            return _dbContext.Shops.Select(p => p.Id).ToListAsync();
        }

        public async Task RefreshStats(int id)
        {
            var shop = await _dbContext.Shops.FindAsync(id);
            shop.LikeCount = _dbContext.ShopReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id).Count();
            shop.DislikeCount = _dbContext.ShopReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id).Count();
            shop.ReportCount = _dbContext.ShopReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id).Count();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCount(int id, ReactionType type)
        {
            var shop = await _dbContext.Shops.FindAsync(id);
            switch (type)
            {
                case ReactionType.Like: return shop.LikeCount;
                case ReactionType.Dislike: return shop.DislikeCount;
                case ReactionType.Report: return shop.ReportCount;
                default: return 0;
            }
        }
    }
}