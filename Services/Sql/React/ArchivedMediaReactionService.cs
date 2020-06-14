using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

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

        public async Task RefreshStats(int id)
        {
            var ArchivedMedia = await _dbContext.ArchivedMedia.FindAsync(id);
            ArchivedMedia.LikeCount = _dbContext.ArchivedMediaReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id).Count();
            ArchivedMedia.DislikeCount = _dbContext.ArchivedMediaReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id).Count();
            ArchivedMedia.ReportCount = _dbContext.ArchivedMediaReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id).Count();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCount(int id, ReactionType type)
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