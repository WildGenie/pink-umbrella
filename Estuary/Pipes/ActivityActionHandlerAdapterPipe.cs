using System;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;

namespace Estuary.Pipes
{
    public class ActivityActionHandlerAdapterPipe : AbstractActivityPipe
    {
        public override Task<BaseObject> Pipe(ActivityDeliveryContext ctx) => HandleRequest(ctx, ctx.item.type, GetArguments);

        private object[] GetArguments(ActivityDeliveryContext arg) => new object[] { arg, arg.item }; // CustomJsonSerializer.TypeOf(
    }
}