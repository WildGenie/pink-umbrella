using System.Threading.Tasks;
using Tides.Core;
using Tides.Models;
using Tides.Services;

namespace PinkUmbrella.Services.Sql.ActivityStreamPipes
{
    public class BindTagsToActivityStream : IActivityStreamPipe
    {
        private readonly ITagService _tags;

        public BindTagsToActivityStream(ITagService tags)
        {
            _tags = tags;
        }

        public async Task<BaseObject> Pipe(BaseObject value, bool isReading)
        {
            if (value.PeerId == 0 && value.objectId.HasValue)
            {
                var tags = await _tags.GetTagsFor(value.objectId.Value, ReactionSubject.Profile, value.ViewerId);
                value.tag = tags;
            }
            return value;
        }
    }
}