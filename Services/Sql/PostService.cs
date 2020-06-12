using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql
{
    public class PostService : IPostService
    {
        private readonly SimpleDbContext _dbContext;

        public PostService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NewPostResult> TryCreatePosts(List<PostModel> post_chain)
        {
            PostModel previous = null;
            foreach (var post in post_chain) {
                var error = await TryCreatePost(post);
                if (error.Error) {
                    return error;
                }
                if (previous != null) {
                    previous.NextInChain = post.Id;
                }
                previous = post;
            }
            if (previous != null) {
                await _dbContext.SaveChangesAsync();
            }
            
            return new NewPostResult() {
                Error = false,
                Posts = post_chain
            };
        }

        private async Task<NewPostResult> TryCreatePost(PostModel post)
        {
            post.WhenCreated = DateTime.UtcNow;
            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();
            return new NewPostResult() {
                Error = false
            };
        }

        public Task<NewPostResult> TryCreateTextPosts(int userId, string[] post_chain, Visibility visibility)
        {
            var posts = post_chain.Select(p => new PostModel() {
                Content = p,
                PostType = PostType.Text,
                Visibility = visibility,
                UserId = userId,
            }).ToList();
            return TryCreatePosts(posts);
        }

        public Task DeletePost(int id, int userId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<PostModel> GetPost(int id)
        {
            return await _dbContext.Posts.FindAsync(id);
        }

        public Task UpdateShadowBanStatus(int id, bool status)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<PostModel>> UserPosts(int userId, int viewerId, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}