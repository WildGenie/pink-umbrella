using System.Linq;
using System.Threading.Tasks;
using Tides.Core;
using Tides.Models;
using Tides.Services;

namespace PinkUmbrella.Services.Sql.ActivityStreamPipes
{
    public class BindSqlReferencesToActivityStream : IActivityStreamPipe
    {
        public Task<BaseObject> Pipe(BaseObject value, bool isReading)
        {
            if (value.PeerId == 0)
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

            if (value.ViewerId.HasValue && isReading)
            {
                //user.Reactions = await _reactions.Get(ReactionSubject.Profile, id, viewerId);
                var reactionTypes = value.Reactions.items.Select(r => r.type).ToHashSet();
                value.HasLiked = reactionTypes.Contains(nameof(ReactionType.Like));
                value.HasDisliked = reactionTypes.Contains(nameof(ReactionType.Dislike));
                value.HasFollowed = reactionTypes.Contains(nameof(ReactionType.Follow));
                value.HasBlocked = reactionTypes.Contains(nameof(ReactionType.Block));
                value.HasReported = reactionTypes.Contains(nameof(ReactionType.Report));
                value.HasBeenBlockedOrReportedByViewer = value.HasReported || value.HasBlocked;

                //var blockOrReport = await _reactions.HasBlockedViewer(ReactionSubject.Profile, id, viewerId);
                //user.HasBeenBlockedOrReportedByPublisher = blockOrReport;

                if (!value.ViewerIsPublisher)
                {
                    // HasBeenBlockedOrReported
                    // var blockOrReport = await _dbContext.ProfileReactions.FirstOrDefaultAsync(r => ((r.ToId == viewerId.Value && r.UserId == media.UserId) || (r.ToId == media.UserId && r.UserId == viewerId.Value) && (r.Type == ReactionType.Block || r.Type == ReactionType.Report)));
                    // media.HasBeenBlockedOrReported =  blockOrReport != null;
                }
                else
                {
                    //media.ViewerIsFollowing = true;
                }
            }
            return Task.FromResult(value);
        }
    }
}