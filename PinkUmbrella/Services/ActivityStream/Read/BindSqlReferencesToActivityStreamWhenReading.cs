using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Services;
using Estuary.Util;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services.ActivityStream.Read
{
    public class BindSqlReferencesToActivityStreamWhenReading : IActivityStreamPipe
    {
        private readonly RedisRepository _redis;
        private readonly StringRepository _strings;
        private readonly SimpleDbContext _db;
        private readonly IActivityStreamContentRepository _contentRepository;

        public BindSqlReferencesToActivityStreamWhenReading(
            RedisRepository redis,
            StringRepository strings,
            SimpleDbContext db,
            IActivityStreamContentRepository contentRepository)
        {
            _redis = redis;
            _strings = strings;
            _db = db;
            _contentRepository = contentRepository;
        }

        public async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            ctx.item.ViewerId = ctx.Filter.viewerId;
            if (ctx.item.obj != null)
            {
                ctx.item.obj.ViewerId = ctx.Filter.viewerId;
            }

            if (ctx.item.PeerId == 0)
            {
                // if (shop.OwnerUser == null)
                // {
                //     shop.OwnerUser = await _users.GetUser(new PublicId(shop.UserId, shop.PeerId), viewerId);
                // }
                // if (value.User == null)
                // {
                //     value.User = await _users.GetUser(value.UserId, value.ViewerId);
                // }
            }

            if (ctx.item.ViewerId.HasValue && ctx.IsReading)
            {
                await BindReactions(ctx);

                if (ctx.item.obj != null && ctx.item.obj.ViewerIsPublisher)
                {
                }
                else
                {
                    //var blockOrReport = await _reactions.HasBlockedViewer(ReactionSubject.Profile, id, viewerId);
                    //user.HasBeenBlockedOrReportedByPublisher = blockOrReport;
                    //media.ViewerIsFollowing = true;
                }
            }
            return null;
        }

        private async Task BindReactions(ActivityDeliveryContext ctx)
        {
            if (ctx.item.actor?.items != null)
            {
                foreach (var actor in ctx.item.actor.items.OfType<ActorObject>())
                {
                    await BindSqlActor(ctx, actor);
                }
            }
            if (ctx.item.obj != null && ctx.item.type != "Undo")
            {
                await BindSqlExtra(ctx, ctx.item.obj);
            }
            if (ctx.item.target?.items != null)
            {
                foreach (var target in ctx.item.target.items)
                {
                    await _contentRepository.BindSqlContent(target);
                    await BindSqlExtra(ctx, target);
                }
            }
        }

        private async Task BindSqlActor(ActivityDeliveryContext ctx, ActorObject bindTo)
        {
            if (bindTo.PublicId != null && bindTo.PublicId.IsLocal && bindTo.PublicId.Id.HasValue)
            {
                var profile = await _db.Users.FindAsync(bindTo.PublicId.Id.Value);
                if (profile != null)
                {
                    bindTo.name = profile.DisplayName;
                    bindTo.Handle = profile.Handle;
                    bindTo.BanExpires = profile.BanExpires;
                    bindTo.BanReason = profile.BanReason;
                    bindTo.summary = profile.Bio;
                    //bindTo.content = content.Content;
                    //bindTo.mediaType = content.MediaType;
                    //bindTo.vis = content.Visibility;
                    bindTo.published = profile.WhenCreated;
                    bindTo.updated = profile.WhenLastUpdated;
                    bindTo.deleted = profile.WhenDeleted;
                    //bindTo.IsMature = profile.IsMature;
                }
            }
        }

        private async Task BindSqlExtra(ActivityDeliveryContext ctx, BaseObject bindTo)
        {
            if (bindTo.ViewerId.HasValue)
            {
                var viewerId = new PublicId(bindTo?.ViewerId.Value, 0);
                viewerId.Type = "Person";
                var reactionsFilter = new ActivityStreamFilter("outbox")
                {
                    id = viewerId, viewerId = bindTo?.ViewerId.Value, targetId = bindTo.PublicId,
                    includeReplies = false,
                }.ReactionsOnly();
                var reactionsBox = ctx.context.GetBox(reactionsFilter);
                bindTo.Reactions = await reactionsBox.Get(reactionsFilter);
                if (bindTo.Reactions.items.Count > 0)
                {
                    var reactionTypes = bindTo.Reactions.items.Select(r => r.type).ToHashSet();
                    if (reactionTypes.Count > 0)
                    {
                        bindTo.HasLiked = reactionTypes.Contains(nameof(ReactionType.Like));
                        bindTo.HasDisliked = reactionTypes.Contains(nameof(ReactionType.Dislike));
                        bindTo.HasUpvoted = reactionTypes.Contains(nameof(ReactionType.Upvote));
                        bindTo.HasDownvoted = reactionTypes.Contains(nameof(ReactionType.Downvote));
                        bindTo.HasFollowed = reactionTypes.Contains(nameof(ReactionType.Follow));
                        bindTo.HasIgnored = reactionTypes.Contains(nameof(ReactionType.Ignore));
                        bindTo.HasBlocked = reactionTypes.Contains(nameof(ReactionType.Block));
                        bindTo.HasReported = reactionTypes.Contains(nameof(ReactionType.Report));
                        bindTo.HasBeenBlockedOrReportedByViewer = bindTo.HasReported || bindTo.HasBlocked;
                    }
                }
            }

            var reactionsSummary = await _redis.Get<ReactionsSummaryModel>(bindTo.id);
            if (reactionsSummary != null)
            {
                bindTo.LikeCount = reactionsSummary.LikeCount;
                bindTo.DislikeCount = reactionsSummary.DislikeCount;
                bindTo.UpvoteCount = reactionsSummary.UpvoteCount;
                bindTo.DownvoteCount = reactionsSummary.DownvoteCount;
                bindTo.FollowCount = reactionsSummary.FollowCount;
                bindTo.IgnoreCount = reactionsSummary.IgnoreCount;
                bindTo.BlockCount = reactionsSummary.BlockCount;
                bindTo.ReportCount = reactionsSummary.ReportCount;
            }
        }
    }
}