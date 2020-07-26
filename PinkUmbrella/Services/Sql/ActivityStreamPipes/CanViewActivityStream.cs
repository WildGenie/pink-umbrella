using System.Threading.Tasks;
using Tides.Core;
using Tides.Models;
using Tides.Services;

namespace PinkUmbrella.Services.Sql.ActivityStreamPipes
{
    public class CanViewActivityStream : IActivityStreamPipe
    {
        public Task<BaseObject> Pipe(BaseObject value, bool isReading)
        {
            if (value.ViewerIsPublisher)
            {
                return Task.FromResult(value);
            }
            
            if (value.HasBeenBlockedOrReportedByPublisher)
            {
                return Task.FromResult<BaseObject>(null);
            }


            if (value.visibility.HasValue)
            {
                switch (value.visibility.Value)
                {
                    case Visibility.HIDDEN: return Task.FromResult<BaseObject>(null);
                    case Visibility.VISIBLE_TO_FOLLOWERS:
                    if (!value.ViewerIsFollowing)
                    {
                        return Task.FromResult<BaseObject>(null);
                    }
                    break;
                    case Visibility.VISIBLE_TO_REGISTERED:
                    if (!value.ViewerId.HasValue)
                    {
                        return Task.FromResult<BaseObject>(null);
                    }
                    break;
                }
                // switch (user.visibility.Value)
                // {
                //     case Visibility.HIDDEN: return false;
                //     case Visibility.VISIBLE_TO_FOLLOWERS:
                //     if (!user.Reactions.items.Any(r => r.type == nameof(ReactionType.Follow)))
                //     {
                //         return false;
                //     }
                //     break;
                //     case Visibility.VISIBLE_TO_REGISTERED:
                //     if (!viewerId.HasValue)
                //     {
                //         return false;
                //     }
                //     break;
                // }
            }
            return Task.FromResult(value);
        }
    }
}