using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Models.Public;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Sql.React
{
    public class ArchivedMediaReactionService : IReactableService
    {
        private readonly SimpleDbContext _dbContext;

        public ArchivedMediaReactionService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string ControllerName => "Archive";

        public ReactionSubject Subject => ReactionSubject.ArchivedMedia;

        public Task<List<int>> GetIds()
        {
            return _dbContext.ArchivedMedia.Select(p => p.Id).ToListAsync();
        }

        public async Task RefreshStats(PublicId id)
        {
            var ArchivedMedia = await _dbContext.ArchivedMedia.FindAsync(id);
            ArchivedMedia.LikeCount = _dbContext.ArchivedMediaReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            ArchivedMedia.DislikeCount = _dbContext.ArchivedMediaReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            ArchivedMedia.ReportCount = _dbContext.ArchivedMediaReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCount(PublicId id, ReactionType type)
        {
            var ArchivedMedia = await _dbContext.ArchivedMedia.FindAsync(id);
            switch (type)
            {
                case ReactionType.Like: return ArchivedMedia.LikeCount;
                case ReactionType.Dislike: return ArchivedMedia.DislikeCount;
                case ReactionType.Report: return ArchivedMedia.ReportCount;
                default: return 0;
            }
        }
    }
}