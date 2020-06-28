using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using PinkUmbrella.Services;

namespace PinkUmbrella.Util
{
    public class ExternalDbMiddleware
    {
        private readonly RequestDelegate _next;

        public ExternalDbMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env, ExternalDbOptions externalDbOptions, IExternalDbContext externalDb)
        {
            var handle = await externalDbOptions.ExtractDbHandle(context);
            if (string.IsNullOrWhiteSpace(handle))
            {
                await _next(context);
            }
            else
            {
                await externalDb.SwitchTo(handle);
                await _next(context);
                externalDb.Dispose();
            }
        }
    }
}