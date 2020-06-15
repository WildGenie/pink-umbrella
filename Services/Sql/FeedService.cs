using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql
{
    public class FeedService: IFeedService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPostService _posts;

        public FeedService(SimpleDbContext dbContext, IPostService posts)
        {
            _dbContext = dbContext;
            _posts = posts;
        }


        // TODO: Fix bugs regarding CanView and user
        public async Task<FeedModel> GetFeedForUser(int userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            var followerIds = await _dbContext.ProfileReactions.Where(r => r.UserId == userId && r.Type == ReactionType.Follow).Select(r => r.ToId).ToListAsync();
            var posts = await _dbContext.Posts.Where(p => p.IsReply == includeReplies && followerIds.Contains(p.UserId)).OrderByDescending(p => p.WhenCreated).ToListAsync();
            var keepers = new List<PostModel>();
            foreach (var p in posts) {
                await _posts.BindReferences(p, viewerId);
                if (_posts.CanView(p, viewerId))
                {
                    keepers.Add(p);
                }
            }
            return new FeedModel() {
                Items = keepers.Skip(pagination.start).Take(pagination.count).ToList(),
                Pagination = pagination,
                RepliesIncluded = includeReplies,
                UserId = userId,
                ViewerId = viewerId,
                Total = keepers.Count()
            };
        }
    }
}