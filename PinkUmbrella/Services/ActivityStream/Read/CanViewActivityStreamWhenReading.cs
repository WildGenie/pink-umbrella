using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Services;
using Tides.Models;

namespace PinkUmbrella.Services.ActivityStream.Read
{
    public class CanViewActivityStreamWhenReading : IActivityStreamPipe
    {
        public async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsReading && !ctx.item.ViewerIsPublisher)
            {
                if (ctx.item.HasBeenBlockedOrReportedByPublisher || (ctx.item.obj != null && ctx.item.obj.HasBeenBlockedOrReportedByPublisher))
                {
                    return null;
                }

                if (ctx.item.to != null)
                {
                    var firstActor = ctx.item.to.items?.OfType<ActorObject>()?.FirstOrDefault();
                    if (firstActor != null)
                    {
                        if (ctx.Filter.viewerId.HasValue && ctx.Filter.viewerId.Value == firstActor.PublicId.Id)
                        {
                            return null;
                        }
                        else
                        {
                            var handle = firstActor.Handle;
                            switch (handle)
                            {
                                case "followers":
                                if (!ctx.item.ViewerIsFollowing)
                                {
                                    return null;
                                }
                                break;
                                case "registered":
                                if (!ctx.item.ViewerId.HasValue)
                                {
                                    return null;
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
                    }
                }
            }
            return null;
        }
    }
}