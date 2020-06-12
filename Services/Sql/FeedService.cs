using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql
{
    public class FeedService : IFeedService
    {
        private readonly SimpleDbContext _dbContext;

        public FeedService(SimpleDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<FeedModel> GetFeedForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination)
        {
            var posts = await _dbContext.Posts.Where(p => p.UserId == userId).Skip(pagination.start).Take(pagination.count).ToListAsync();
            return new FeedModel() {
                Posts = posts,
                Pagination = pagination,
                RepliesIncluded = includeReplies,
                UserId = userId,
                ViewerId = viewerId,
                TotalPosts = _dbContext.Posts.Count()
            };
        }
    }
}