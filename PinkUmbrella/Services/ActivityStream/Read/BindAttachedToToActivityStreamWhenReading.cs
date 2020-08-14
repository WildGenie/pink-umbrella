using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;
using static Estuary.Objects.Common;

namespace PinkUmbrella.Services.Sql.ActivityStream.Read
{
    public class BindAttachedToToActivityStreamWhenReading : IActivityStreamPipe
    {
        private readonly IArchiveService _archive;

        public BindAttachedToToActivityStreamWhenReading(IArchiveService archive)
        {
            _archive = archive;
        }

        public async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.item.obj is Document media)
            {
                // if (media.RelatedPost == null && media.RelatedPostId.HasValue)
                // {
                //     // TODO: give media peer ids
                //     media.RelatedPost = await _posts.GetPost(new PublicId(media.RelatedPostId.Value, 0), viewerId); 
                // }
            }
            await Task.Delay(1);
            return ctx.item.obj;
        }
    }
}