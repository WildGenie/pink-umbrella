using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using seattle.Models;
using seattle.Repositories;
using System.Collections.Generic;

namespace seattle.Services.Sql
{
    public class FeedService : IFeedService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPostService _posts;

        public FeedService(SimpleDbContext dbContext, IPostService posts) {
            _dbContext = dbContext;
            _posts = posts;
        }

        public async Task<FeedModel> GetFeedForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination)
        {
            var posts = await _dbContext.Posts.Where(p => p.UserId == userId).Skip(pagination.start).Take(pagination.count).ToListAsync();
            foreach (var p in posts) {
                p.Mentions = await _dbContext.Mentions.Where(m => m.PostId == p.Id).ToListAsync();
            }
            return new FeedModel() {
                Posts = posts,
                Pagination = pagination,
                RepliesIncluded = includeReplies,
                UserId = userId,
                ViewerId = viewerId,
                TotalPosts = _dbContext.Posts.Count()
            };
        }

        public async Task<FeedModel> GetMentionsForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination)
        {
            var mentions =  _dbContext.Mentions.Where(m => m.MentionedUserId == userId);
            var paginated = await mentions.Skip(pagination.start).Take(pagination.count).ToListAsync();
            var posts = new List<PostModel>();
            foreach (var p in mentions) {
                posts.Add(await _posts.GetPost(p.PostId));
            }
            return new FeedModel() {
                Posts = posts,
                Pagination = pagination,
                RepliesIncluded = includeReplies,
                UserId = userId,
                ViewerId = viewerId,
                TotalPosts = mentions.Count()
            };
        }

        public Task<FeedModel> GetPostsForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}