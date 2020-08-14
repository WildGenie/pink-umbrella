using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Repositories;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;
using static Estuary.Activities.Common;
using static Estuary.Objects.Common;
using Estuary.Actors;
using Estuary.Services;
using Estuary.Util;
using System;
using Estuary.Objects;

namespace PinkUmbrella.Services.Sql
{
    public class PostService : IPostService
    {
        private readonly StringRepository _strings;
        private readonly IPublicProfileService _users;
        private readonly ITagService _tags;
        private readonly INotificationService _notifications;
        private readonly IActivityStreamRepository _activityStreams;

        public PostService(StringRepository strings, IPublicProfileService users, ITagService tags, INotificationService notifications, IActivityStreamRepository activityStreams)
        {
            _strings = strings;
            _users = users;
            _tags = tags;
            _notifications = notifications;
            _activityStreams = activityStreams;
        }

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

        public async Task<BaseObject> TryCreateTextPost(ActorObject publisher, string content, Visibility visibility)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new Error
                {
                    statusCode = 403,
                    errorCode = 403,
                    summary = "content is empty"
                };
            }
            var audience = visibility.ToAudience(publisher ?? throw new ArgumentNullException(nameof(publisher))).
                            IntoNewList<BaseObject>().ToCollection();
            var post = new Create
            {
                obj = new Note()
                {
                    content = content,
                    audience = audience
                },
                actor = publisher.IntoNewList<BaseObject>().ToCollection(),
                to = audience,
            };
            return (await _activityStreams.Post("outbox", post)) ?? post;
        }

        public async Task<BaseObject> GetMentionsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            // PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination
            var mentions = await _activityStreams.Get(new ActivityStreamFilter("inbox")
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

        public async Task<BaseObject> GetPostsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            if (userId.PeerId == 0)
            {
                return await _activityStreams.Get(new ActivityStreamFilter("outbox")
                {
                    userId = userId.Id, includeReplies = includeReplies,
                });
            }
            else
            {
                return new CollectionObject();
            }
        }
    }
}