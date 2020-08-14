using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Pipes;
using Estuary.Services;
using static Estuary.Activities.Common;
using static Estuary.Objects.Common;

namespace PinkUmbrella.Services.ActivityStream.Write.Inbox
{
    public class InboxActivityActionObjectHandlerWhenWritingPipe: ActivityActionObjectHandlerAdapterPipe
    {
        public override Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsWriting && ctx.box.filter.index == "inbox")
            {
                return base.Pipe(ctx);
            }
            else
            {
                return Task.FromResult<BaseObject>(null);
            }
        }

        public async Task<BaseObject> CreateNote(ActivityDeliveryContext ctx, Create create, Note note)
        {
//             // if (ctx.box.name != "sharedOutbox" && ctx.box.name != "sharedOutbox" &&
//             //     (ctx.item.visibility ?? Visibility.VISIBLE_TO_WORLD) == Visibility.VISIBLE_TO_WORLD)
//             // {
//             //     ctx.Forward(ctx.item, new Tides.Models.ActivityStreamFilter
//             //     {
//             //         index = "sharedOutbox",
//             //     });
//             // }
            return null;
        }
    }
}