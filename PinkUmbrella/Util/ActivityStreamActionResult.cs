using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Estuary.Core;
using Estuary.Objects;

namespace PinkUmbrella.Util
{
    public class ActivityStreamActionResult : JsonResult
    {
        public ActivityStreamActionResult(BaseObject result): base(result)
        {
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add("Content-Type", "application/activity+json");
            if (Value is Error err)
            {
                context.HttpContext.Response.StatusCode = err.errorCode;
            }
            else
            {
                context.HttpContext.Response.StatusCode = 200;
            }

            return base.ExecuteResultAsync(context);
        }
    }
}