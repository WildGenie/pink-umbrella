using System.Threading.Tasks;
using Tides.Core;
using Tides.Models;
using Tides.Services;
using static Tides.Objects.Common;

namespace PinkUmbrella.Services.Sql.ActivityStreamPipes
{
    public class BindAttachedToToActivityStream : IActivityStreamPipe
    {
        private readonly IArchiveService _archive;

        public BindAttachedToToActivityStream(IArchiveService archive)
        {
            _archive = archive;
        }

        public async Task<BaseObject> Pipe(BaseObject value, bool isReading)
        {
            if (value is Document media)
            {
                // if (media.RelatedPost == null && media.RelatedPostId.HasValue)
                // {
                //     // TODO: give media peer ids
                //     media.RelatedPost = await _posts.GetPost(new PublicId(media.RelatedPostId.Value, 0), viewerId); 
                // }
            }
            await Task.Delay(1);
            return value;
        }
    }
}