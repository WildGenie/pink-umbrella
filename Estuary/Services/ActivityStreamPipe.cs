using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Estuary.Core;

namespace Estuary.Services
{
    public class ActivityStreamPipe: List<IActivityStreamPipe>, IHazActivityStreamPipe, IActivityStreamPipe
    {
        public ActivityStreamPipe()
        {
        }

        public ActivityStreamPipe(IEnumerable<IActivityStreamPipe> collection) : base(collection)
        {
        }

        public ActivityStreamPipe(int capacity) : base(capacity)
        {
        }

        public ActivityStreamPipe GetPipe() => this;

        public async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsReading || (ctx.IsWriting && !ctx.HasWritten))
            {
                foreach (var handler in this)
                {
                    var objTask = handler.Pipe(ctx);
                    var obj = await (objTask ?? throw new Exception($"{handler.GetType().FullName}.Pipe() returned null task"));
                    if (ctx.item == null)
                    {
                        return null;
                    }
                    else if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            else
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    var obj = await this[i].Pipe(ctx);
                    if (ctx.item == null)
                    {
                        return null;
                    }
                    else if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }
    }
}