using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Models.Public;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Sql.React
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

        public async Task RefreshStats(PublicId id)
        {
            var shop = await _dbContext.Shops.FindAsync(id);
            shop.LikeCount = _dbContext.ShopReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            shop.DislikeCount = _dbContext.ShopReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            shop.ReportCount = _dbContext.ShopReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCount(PublicId id, ReactionType type)
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