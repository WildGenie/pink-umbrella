using System;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;

namespace Estuary.Pipes
{
    public class ActivityObjectHandlerAdapterPipe : AbstractActivityPipe
    {
        public override Task<BaseObject> Pipe(ActivityDeliveryContext ctx) => HandleRequest(ctx, ctx.item.obj?.type, GetArguments);

        private object[] GetArguments(ActivityDeliveryContext arg) => new object[] { arg, arg.item, arg.item.obj };
    }
}