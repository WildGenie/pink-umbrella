using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql.React
{
    public class PostReactionService : IReactableService
    {
        private readonly SimpleDbContext _dbContext;

        public PostReactionService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string ControllerName => "Posts";

        public ReactionSubject Subject => ReactionSubject.Post;

        public Task<List<int>> GetIds()
        {
            return _dbContext.Posts.Select(p => p.Id).ToListAsync();
        }

        public async Task RefreshStats(int id)
        {
            var post = await _dbContext.Posts.FindAsync(id);
            post.LikeCount = _dbContext.PostReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id).Count();
            post.DislikeCount = _dbContext.PostReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id).Count();
            post.ReportCount = _dbContext.PostReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id).Count();
            post.BlockCount = _dbContext.PostReactions.Where(r => r.Type == ReactionType.Block && r.ToId == id).Count();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCount(int id, ReactionType type)
        {
            var post = await _dbContext.Posts.FindAsync(id);
            switch (type)
            {
                case ReactionType.Like: return post.LikeCount;
                case ReactionType.Dislike: return post.DislikeCount;
                case ReactionType.Report: return post.ReportCount;
                case ReactionType.Block: return post.BlockCount;
                default: return 0;
            }
        }
    }
}