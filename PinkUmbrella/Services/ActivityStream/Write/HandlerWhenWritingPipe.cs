using System;
using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Objects;
using Estuary.Pipes;
using Estuary.Services;
using Estuary.Util;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.ActivityStream.Write.Outbox
{
    public class HandlerWhenWritingPipe: AbstractActivityPipe
    {
        private readonly IObjectReferenceService _ids;
        private readonly IObjectReferenceService _handles;
        private readonly StringRepository _strings;
        private readonly ITagService _tags;
        private readonly RedisRepository _redis;

        public HandlerWhenWritingPipe(
            IObjectReferenceService ids,
            IObjectReferenceService handles,
            StringRepository strings,
            ITagService tags,
            RedisRepository redis)
        {
            _ids = ids;
            _handles = handles;
            _strings = strings;
            _tags = tags;
            _redis = redis;
        }

        public override async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.item.from == null || ctx.item.from.items.Count == 0)
            {
                return new Error { errorCode = 403, statusCode = 403, summary = "Missing from in item" };
            }
            else if (ctx.item is ActivityObject activity && activity.obj != null && (activity.obj.from == null || activity.obj.from.items.Count == 0))
            {
                return new Error { errorCode = 403, statusCode = 403, summary = "Missing from in item.obj" };
            }
            return null;
        }
    }
}