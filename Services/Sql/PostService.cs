using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Models.AhPushIt;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql
{
    public class PostService : IPostService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly StringRepository _strings;
        private readonly IUserProfileService _users;
        private readonly ITagService _tags;
        private readonly INotificationService _notifications;

        public PostService(SimpleDbContext dbContext, StringRepository strings, IUserProfileService users, ITagService tags, INotificationService notifications)
        {
            _dbContext = dbContext;
            _strings = strings;
            _users = users;
            _tags = tags;
            _notifications = notifications;
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

            bool postInsertChanges = false;

            if (post.Visibility != Visibility.HIDDEN)
            {
                await ExtractMentions(post);
                if (post.Mentions.Any())
                {
                    _dbContext.Mentions.AddRange(post.Mentions);
                    postInsertChanges = true;
                }
            }

            await ExtractTags(post);
            if (post.Tags.Any())
            {
                await _tags.Save(ReactionSubject.Post, post.Tags, post.UserId, post.Id);
                postInsertChanges = true;
            }

            if (postInsertChanges)
            {
                await _dbContext.SaveChangesAsync();

                await SendOutNotifications(post);
            }

            return new NewPostResult() {
                Error = false
            };
        }

        private async Task SendOutNotifications(PostModel post)
        {
            if (post.PostType == PostType.Text)
            {
                await SendOutNotificationsToTagFollowers(post);
                await SendOutNotificationsToPosterFollowers(post);
                await SendOutNotificationsToMentioned(post);
            }
        }

        private async Task SendOutNotificationsToTagFollowers(PostModel post)
        {
            var tagIds = post.Tags.Select(t => t.Id).ToArray();
            var tagFollowers = await _dbContext.FollowingPostTags.Where(f => tagIds.Contains(f.TagId)).Select(u => u.UserId).Distinct().ToArrayAsync();
            if (tagFollowers.Length > 0)
            {
                await _notifications.Publish(new Models.AhPushIt.Notification() {
                    FromUserId = post.UserId,
                    SubjectId = post.Id,
                    Priority = NotificationPriority.Normal,
                    Subject = ReactionSubject.Post,
                    Type = NotificationType.TEXT_POST_FOLLOWED_TAG,
                }, tagFollowers);
            }
        }

        private async Task SendOutNotificationsToPosterFollowers(PostModel post)
        {
            var followers = await _users.GetFollowers(post.UserId, post.UserId);
            if (followers.Count > 0)
            {
                await _notifications.Publish(new Models.AhPushIt.Notification() {
                    FromUserId = post.UserId,
                    SubjectId = post.Id,
                    Priority = NotificationPriority.Normal,
                    Subject = ReactionSubject.Post,
                    Type = NotificationType.TEXT_POST_FOLLOWED_USER,
                }, followers.Select(u => u.Id).ToArray());
            }
        }

        private async Task SendOutNotificationsToMentioned(PostModel post)
        {
            if (post.Mentions.Count > 0)
            {
                await _notifications.Publish(new Models.AhPushIt.Notification() {
                    FromUserId = post.UserId,
                    SubjectId = post.Id,
                    Priority = NotificationPriority.Normal,
                    Subject = ReactionSubject.Post,
                    Type = NotificationType.TEXT_POST_MENTION,
                }, post.Mentions.Select(u => u.MentionedUserId).ToArray());
            }
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

        public async Task<PostModel> GetPost(int id, int? viewerId)
        {
            var p = await _dbContext.Posts.FindAsync(id);
            await BindReferences(p, viewerId);
            if (CanView(p, viewerId))
            {
                return p;
            }
            else
            {
                return null;
            }
        }

        public bool CanView(PostModel p, int? viewerId)
        {
            if (p.ViewerIsPoster)
            {
                return true;
            }
            
            if (p.HasBeenBlockedOrReported)
            {
                return false;
            }

            switch (p.Visibility)
            {
                case Visibility.HIDDEN: return false;
                case Visibility.VISIBLE_TO_FOLLOWERS:
                if (!p.ViewerIsFollowing)
                {
                    return false;
                }
                break;
                case Visibility.VISIBLE_TO_REGISTERED:
                if (!viewerId.HasValue)
                {
                    return false;
                }
                break;
            }
            return true;
        }

        public async Task BindReferences(PostModel p, int? viewerId)
        {
            if (p.User == null)
            {
                p.User = await _users.GetUser(p.UserId, viewerId);
            }

            p.ViewerId = viewerId;
            var mentions = await _dbContext.Mentions.Where(m => m.PostId == p.Id).ToListAsync();
            foreach (var m in mentions)
            {
                m.MentionedUser = await _users.GetUser(m.MentionedUserId, viewerId);
            }
            p.Mentions = mentions.Where(m => m.MentionedUser != null).ToList();

            if (viewerId.HasValue)
            {
                p.Reactions = await _dbContext.PostReactions.Where(r => r.UserId == viewerId.Value && r.ToId == p.Id).ToListAsync();
                if (!p.ViewerIsPoster)
                {
                    var reactions = p.Reactions.Select(r => r.Type).ToHashSet();
                    p.HasLiked = reactions.Contains(ReactionType.Like);
                    p.HasDisliked = reactions.Contains(ReactionType.Dislike);
                    p.HasBlocked = reactions.Contains(ReactionType.Block);
                    p.HasReported = reactions.Contains(ReactionType.Report);

                    p.ViewerIsFollowing = (await _dbContext.ProfileReactions.FirstOrDefaultAsync(r => r.ToId == p.UserId && r.UserId == viewerId.Value && r.Type == ReactionType.Follow)) != null;
                
                    var blockOrReport = await _dbContext.ProfileReactions.FirstOrDefaultAsync(r => ((r.ToId == viewerId.Value && r.UserId == p.UserId) || (r.ToId == p.UserId && r.UserId == viewerId.Value) && (r.Type == ReactionType.Block || r.Type == ReactionType.Report)));
                    p.HasBeenBlockedOrReported =  blockOrReport != null; // p.Reactions.Any(r => r.Type == ReactionType.Block || r.Type == ReactionType.Report)
                }
                else
                {
                    p.ViewerIsFollowing = true;
                }
            }

            if (p.Tags == null)
            {
                p.Tags = new List<TagModel>();
            }
            else if (p.Tags.Count == 0)
            {
                p.Tags = await _tags.GetTagsFor(p.Id, ReactionSubject.Shop, viewerId);
            }
        }

        public async Task UpdateShadowBanStatus(int id, bool status)
        {
            var p = await _dbContext.Posts.FindAsync(id);
            p.ShadowBanned = status;
            await _dbContext.SaveChangesAsync();
        }

        private async Task ExtractMentions(PostModel p)
        {
            var handles = _strings.ExtractMentionsRegex.Matches(p.Content).Cast<Match>().Select(m => m.Value.Substring(1)).ToList();
            var mentionedUserIds = new List<int>();

            foreach (var handle in handles) {
                var user = await _users.GetUser(handle, p.UserId);
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

        private async Task ExtractTags(PostModel p)
        {
            var handles = _strings.ExtractTagsRegex.Matches(p.Content).Cast<Match>().Select(m => m.Value.Substring(1)).ToList();
            var tagIds = new List<int>();

            foreach (var handle in handles) {
                var tag = await _tags.TryGetOrCreateTag(new TagModel() { Tag = handle }, p.UserId);
                if (tag != null) {
                    if (!tagIds.Contains(tag.Id))
                    {
                        p.Tags.Add(tag);
                    }
                }
            }
        }

        public async Task<FeedModel> GetMentionsForUser(int userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            var mentions =  _dbContext.Mentions.Where(m => m.MentionedUserId == userId && m.Post.IsReply == includeReplies);
            var paginated = await mentions.OrderByDescending(p => p.WhenMentioned).ToListAsync();
            var posts = new List<PostModel>();
            foreach (var p in mentions) {
                var post = await GetPost(p.PostId, viewerId);
                if (post != null)
                {
                    posts.Add(post);
                }
            }
            return new FeedModel() {
                Items = posts.Skip(pagination.start).Take(pagination.count).ToList(),
                Pagination = pagination,
                RepliesIncluded = includeReplies,
                UserId = userId,
                ViewerId = viewerId,
                Total = posts.Count()
            };
        }

        public async Task<FeedModel> GetPostsForUser(int userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            var posts = _dbContext.Posts.Where(p => p.UserId == userId && p.IsReply == includeReplies);
            var paginated = await posts.OrderByDescending(p => p.WhenCreated).ToListAsync();
            var keepers = new List<PostModel>();
            foreach (var p in posts) {
                await BindReferences(p, viewerId);
                if (CanView(p, viewerId))
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

        public async Task<PaginatedModel<PostModel>> GetMostReportedPosts()
        {
            var posts = _dbContext.Posts.Where(p => p.ReportCount > 0).OrderByDescending(p => p.ReportCount);
            return await ToPaginatedModel(posts);
        }
        
        public async Task<PaginatedModel<PostModel>> GetMostBlockedPosts()
        {
            var posts = _dbContext.Posts.Where(p => p.BlockCount > 0).OrderByDescending(p => p.BlockCount);
            return await ToPaginatedModel(posts);
        }

        public async Task<PaginatedModel<PostModel>> GetMostDislikedPosts()
        {
            var posts = _dbContext.Posts.Where(p => p.DislikeCount > 0).OrderByDescending(p => p.DislikeCount);
            return await ToPaginatedModel(posts);
        }

        private async Task<PaginatedModel<PostModel>> ToPaginatedModel(IQueryable<PostModel> posts)
        {
            foreach (var p in posts)
            {
                await BindReferences(p, null);
            }
            return new PaginatedModel<PostModel>()
            {
                Items = await posts.Take(10).ToListAsync(),
                Total = posts.Count(),
                Pagination = new PaginationModel(),
            };
        }
    }
}