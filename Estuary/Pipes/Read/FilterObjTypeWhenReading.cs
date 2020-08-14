using System;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;

namespace Estuary.Pipes.Read
{
    public class FilterObjTypeWhenReading : IActivityStreamPipe
    {
        public Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsReading && ctx.item.obj != null && ctx.Filter != null &&
                ctx.Filter.objectTypes != null && ctx.Filter.objectTypes.Length > 0)
            {
                if (Array.IndexOf(ctx.Filter.objectTypes, ctx.item.obj.type) < 0 &&
                    Array.IndexOf(ctx.Filter.objectTypes, ctx.item.obj.BaseType) < 0)
                {
                    ctx.item = null;
                }
            }
            return Task.FromResult<BaseObject>(null);
        }
    }
}