using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql
{
    public class PostService : IPostService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly StringRepository _strings;
        private readonly IUserProfileService _users;

        public PostService(SimpleDbContext dbContext, StringRepository strings, IUserProfileService users)
        {
            _dbContext = dbContext;
            _strings = strings;
            _users = users;
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

            await ExtractMentions(post);

            _dbContext.Mentions.AddRange(post.Mentions);
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
            var p = await _dbContext.Posts.FindAsync(id);
            p.Mentions = await _dbContext.Mentions.Where(m => m.PostId == id).ToListAsync();
            return p;
        }

        public Task UpdateShadowBanStatus(int id, bool status)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<PostModel>> UserPosts(int userId, int viewerId, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }

        private async Task ExtractMentions(PostModel p)
        {
            var handles = _strings.ExtractMentionsRegex.Matches(p.Content).Cast<Match>().Select(m => m.Value.Substring(1)).ToList();
            var mentionedUserIds = new List<int>();

            foreach (var handle in handles) {
                var user = await _users.GetUser(handle);
                if (user != null) {
                    if (!mentionedUserIds.Contains(user.Id))
                    {
                        mentionedUserIds.Add(user.Id);
                        p.Mentions.Add(new MentionModel() {
                            WhenMentioned = DateTime.UtcNow,
                            PostId = p.Id,
                            Post = p,
                            MentionedUserId = user.Id,
                            MentionedUser = user
                        });
                    }
                }
            }
        }
    }
}