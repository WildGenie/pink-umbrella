using System.Linq;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;
using Estuary.Util;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Models;

namespace PinkUmbrella.Services.ActivityStream.Read
{
    public class BindSqlReferencesToActivityStreamWhenReading : IActivityStreamPipe
    {
        private readonly RedisRepository _redis;
        private readonly StringRepository _strings;

        public BindSqlReferencesToActivityStreamWhenReading(RedisRepository redis, StringRepository strings)
        {
            _redis = redis;
            _strings = strings;
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
            if (ctx.item.obj != null)
            {
                var reactionsFilter = new ActivityStreamFilter("sharedInbox") { id = ctx.item.obj?.id }.ReactionsOnly();
                var reactionsBox = ctx.context.GetBox(reactionsFilter);
                ctx.item.obj.Reactions = await reactionsBox.Get(reactionsFilter);
                if (ctx.item.obj.Reactions.items.Count > 0)
                {
                    var reactionTypes = ctx.item.obj.Reactions.items.Select(r => r.type).ToHashSet();
                    if (reactionTypes.Count > 0)
                    {
                        ctx.item.obj.HasLiked = reactionTypes.Contains(nameof(ReactionType.Like));
                        ctx.item.obj.HasDisliked = reactionTypes.Contains(nameof(ReactionType.Dislike));
                        ctx.item.obj.HasUpvoted = reactionTypes.Contains(nameof(ReactionType.Upvote));
                        ctx.item.obj.HasDownvoted = reactionTypes.Contains(nameof(ReactionType.Downvote));
                        ctx.item.obj.HasFollowed = reactionTypes.Contains(nameof(ReactionType.Follow));
                        ctx.item.obj.HasIgnored = reactionTypes.Contains(nameof(ReactionType.Ignore));
                        ctx.item.obj.HasBlocked = reactionTypes.Contains(nameof(ReactionType.Block));
                        ctx.item.obj.HasReported = reactionTypes.Contains(nameof(ReactionType.Report));
                        ctx.item.obj.HasBeenBlockedOrReportedByViewer = ctx.item.obj.HasReported || ctx.item.obj.HasBlocked;
                    }
                }

                var reactionsSummary = await _redis.Get<ReactionsSummaryModel>(ctx.item.obj.id);
                if (reactionsSummary != null)
                {
                    ctx.item.obj.LikeCount = reactionsSummary.LikeCount;
                    ctx.item.obj.DislikeCount = reactionsSummary.DislikeCount;
                    ctx.item.obj.UpvoteCount = reactionsSummary.UpvoteCount;
                    ctx.item.obj.DownvoteCount = reactionsSummary.DownvoteCount;
                    ctx.item.obj.FollowCount = reactionsSummary.FollowCount;
                    ctx.item.obj.IgnoreCount = reactionsSummary.IgnoreCount;
                    ctx.item.obj.BlockCount = reactionsSummary.BlockCount;
                    ctx.item.obj.ReportCount = reactionsSummary.ReportCount;
                }
            }
        }
    }
}