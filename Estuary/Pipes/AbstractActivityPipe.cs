using System;
using System.Reflection;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;

namespace Estuary.Pipes
{
    public abstract class AbstractActivityPipe: IActivityStreamPipe
    {
        protected async Task<BaseObject> HandleResult(object res)
        {
            if (res is BaseObject baseObject)
            {
                return baseObject;
            }
            else if (res is Task<BaseObject> task)
            {
                return await task;
            }
            else if (res == null)
            {
                return null;
            }
            else
            {
                throw new Exception();
            }
        }

        protected async Task<BaseObject> HandleRequest(ActivityDeliveryContext ctx, string handlerName, Func<ActivityDeliveryContext, object[]> getArguments)
        {
            var handler = this.GetType().GetMember(handlerName, BindingFlags.Instance | BindingFlags.Public);
            if (handler != null && handler.Length == 1)
            {
                var res = this.GetType().InvokeMember(handlerName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, this, getArguments != null ? getArguments.Invoke(ctx) : new object[] { ctx });
                return await HandleResult(res);
            }
            else
            {
                return null;
                //throw new Exception($"Cannot handle {activityAction}");
            }
        }

        public abstract Task<BaseObject> Pipe(ActivityDeliveryContext ctx);
    }
}