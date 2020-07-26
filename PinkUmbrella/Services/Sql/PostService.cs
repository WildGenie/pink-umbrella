using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;
using Tides.Services;
using static Tides.Activities.Common;
using static Tides.Objects.Common;

namespace PinkUmbrella.Services.Sql
{
    public class PostService : IPostService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly StringRepository _strings;
        private readonly IPublicProfileService _users;
        private readonly ITagService _tags;
        private readonly INotificationService _notifications;
        private readonly IActivityStreamRepository _activityStreams;

        public PostService(SimpleDbContext dbContext, StringRepository strings, IPublicProfileService users, ITagService tags, INotificationService notifications, IActivityStreamRepository activityStreams)
        {
            _dbContext = dbContext;
            _strings = strings;
            _users = users;
            _tags = tags;
            _notifications = notifications;
            _activityStreams = activityStreams;
        }

        // public async Task<NewPostResult> TryCreatePost(BaseObject post)
        // {
        //     BaseObject previous = null;
        //     foreach (var post in post_chain) {
        //         var error = await TryCreatePost(post);
        //         if (error.Error) {
        //             return error;
        //         }
        //         if (previous != null) {
        //             previous.replies.Add(post);
        //         }
        //         previous = post;
        //     }
        //     if (previous != null) {
        //         await _dbContext.SaveChangesAsync();
        //     }
            
        //     return new NewPostResult() {
        //         Error = false,
        //         Posts = post_chain
        //     };
        // }

        // public async Task<NewPostResult> TryCreatePost(BaseObject post)
        // {
        //     post.published = DateTime.UtcNow;
        //     _dbContext.Posts.Add(post);
        //     await _dbContext.SaveChangesAsync();

        //     bool postInsertChanges = false;

        //     await ExtractTags(post);
        //     if (post.Tags.Any())
        //     {
        //         await _tags.Save(ReactionSubject.Post, post.Tags, post.UserId, post.Id);
        //         postInsertChanges = true;
        //     }

        //     if (postInsertChanges)
        //     {
        //         await _dbContext.SaveChangesAsync();

        //         await SendOutNotifications(post);
        //     }

        //     return new NewPostResult() {
        //         Error = false
        //     };
        // }

        // private async Task SendOutNotifications(BaseObject post)
        // {
        //     if (post.PostType == PostType.Text)
        //     {
        //         await SendOutNotificationsToTagFollowers(post);
        //         await SendOutNotificationsToPosterFollowers(post);
        //         await SendOutNotificationsToMentioned(post);
        //     }
        // }

        // private async Task SendOutNotificationsToTagFollowers(BaseObject post)
        // {
        //     var tagIds = post.Tags.Select(t => t.Tag).ToArray();
        //     var tagFollowers = await _dbContext.FollowingPostTags.Where(f => tagIds.Contains(f.Tag)).Select(u => u.UserId).Distinct().ToArrayAsync();
        //     if (tagFollowers.Length > 0)
        //     {
        //         await _notifications.Publish(new Models.AhPushIt.Notification() {
        //             FromUserId = post.UserId,
        //             SubjectId = post.Id,
        //             Priority = NotificationPriority.Normal,
        //             Subject = ReactionSubject.Post,
        //             Type = NotificationType.TEXT_POST_FOLLOWED_TAG,
        //         }, tagFollowers.Select(id => new PublicId(id, 0)).ToArray());
        //     }
        // }

        // private async Task SendOutNotificationsToPosterFollowers(BaseObject post)
        // {
        //     var followers = await _users.GetFollowers(new PublicId(post.UserId, post.PeerId), post.UserId);
        //     if (followers.Length > 0)
        //     {
        //         await _notifications.Publish(new Models.AhPushIt.Notification() {
        //             FromUserId = post.UserId,
        //             SubjectId = post.Id,
        //             Priority = NotificationPriority.Normal,
        //             Subject = ReactionSubject.Post,
        //             Type = NotificationType.TEXT_POST_FOLLOWED_USER,
        //         }, followers.Select(u => u.PublicId).ToArray());
        //     }
        // }

        // private async Task SendOutNotificationsToMentioned(BaseObject post)
        // {
        //     if (post.Mentions.Count > 0)
        //     {
        //         await _notifications.Publish(new Models.AhPushIt.Notification() {
        //             FromUserId = post.UserId,
        //             SubjectId = post.Id,
        //             Priority = NotificationPriority.Normal,
        //             Subject = ReactionSubject.Post,
        //             Type = NotificationType.TEXT_POST_MENTION,
        //         }, post.Mentions.Select(u => new PublicId(u.MentionedUserId, u.MentionedUserPeerId)).ToArray());
        //     }
        // }

        public async Task<BaseObject> TryCreateTextPost(int userId, string content, Visibility visibility)
        {
            var post = new Create
            {
                obj = new Note()
                {
                    content = content,
                    visibility = visibility,
                },
                actor = new CollectionObject
                {
                    items = new List<BaseObject>
                    {
                        await _users.GetUser(new PublicId(userId, 0), userId)
                    }
                }
            };
            return await _activityStreams.Write(post);
        }


        private async Task ExtractTags(BaseObject p)
        {
            var handles = _strings.ExtractTagsRegex.Matches(p.content).Cast<Match>().Select(m => m.Value.Substring(1)).ToList();
            var tagIds = new HashSet<string>();

            foreach (var handle in handles) {
                var tag = await _tags.TryGetOrCreateTag(new BaseObject(null) { content = handle }, p.UserId);
                if (tag != null) {
                    if (!tagIds.Contains(tag.content))
                    {
                        p.tag.Add(tag);
                        tagIds.Add(tag.content);
                    }
                }
            }
        }

        public async Task<CollectionObject> GetMentionsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            var mentions = await _activityStreams.GetMentions(new ActivityStreamFilter
            {
                userId = userId.Id, peerId = userId.PeerId, viewerId = viewerId, includeReplies = includeReplies
            });
            //_dbContext.Mentions.Where(m => m.MentionedUserId == userId.Id && m.MentionedUserPeerId == userId.PeerId && m.Post.IsReply == includeReplies);
            //var paginated = await mentions.OrderByDescending(p => p.WhenMentioned).ToListAsync();
            //var posts = new List<BaseObject>();
            // foreach (var p in mentions.items.OfType<Mention>()) {
            //     var post = await _activityStreams.GetPost(new ActivityStreamFilter { publicId = p.inReplyTo.first.PublicId, viewerId = viewerId });
            //     if (post != null)
            //     {
            //         posts.Add(post);
            //     }
            // }
            // return new CollectionObject() {
            //     items = posts.Skip(pagination.start).Take(pagination.count).ToList(),
            //     totalItems = posts.Count()
            // };
            return mentions;
        }

        public async Task<CollectionObject> GetPostsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            //var keepers = new List<BaseObject>();
            if (userId.PeerId == 0)
            {
                var posts = await _activityStreams.GetPosts(new ActivityStreamFilter { userId = userId.Id, includeReplies = includeReplies });
                //var paginated = await posts.OrderByDescending(p => p.Id).ToListAsync();
                // return new CollectionObject() {
                //     items = posts.Skip(pagination.start).Take(pagination.count).ToList(),
                //     // Pagination = pagination,
                //     // RepliesIncluded = includeReplies,
                //     totalItems = posts.Count()
                // };
                return posts;
            }
            else
            {
                return new CollectionObject();
            }
        }
    }
}