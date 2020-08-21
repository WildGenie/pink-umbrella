using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Services;
using Tides.Models;
using static Estuary.Activities.Common;

namespace PinkUmbrella.Services.ActivityStream.Read
{
    public class CanViewActivityStreamWhenReading : IActivityStreamPipe
    {
        public async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsReading)
            {
                if (ctx.Filter.performUndos && ctx.item is Undo undo)
                {
                    ctx.Undos.Add(undo.obj.id);
                    ctx.item = null;
                    return null;
                }
                else if (ctx.Filter.performUndos && ctx.Undos.Remove(ctx.item.id))
                {
                    ctx.item = null;
                    return null;
                }
                else if (ctx.item.ViewerIsPublisher)
                {
                    return null;
                }
                else if (ctx.item.HasBeenBlockedOrReportedByPublisher || (ctx.item.obj != null && ctx.item.obj.HasBeenBlockedOrReportedByPublisher))
                {
                    return null;
                }
                else if (ctx.item.to != null)
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