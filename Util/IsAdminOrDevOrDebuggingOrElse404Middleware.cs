using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;

namespace PinkUmbrella.Util
{
    public class IsAdminOrDevOrDebuggingOrElse404Middleware
    {
        private readonly RequestDelegate _next;

        public IsAdminOrDevOrDebuggingOrElse404Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env)
        {
            if (!context.Request.Path.StartsWithSegments("/Admin") || (Debugger.IsAttached || context.User.IsInRole("dev") || context.User.IsInRole("admin")))
            {
                await _next(context);
            }
            else
            {
                context.Response.Redirect($"/Error/404", permanent: false);
            }
        }
    }
}