using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PinkUmbrella.Services;
using PinkUmbrella.Models;

namespace PinkUmbrella.Util
{
    public class LogErrorRedirectProdMiddleware
    {
        private readonly RequestDelegate _next;

        public LogErrorRedirectProdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env, UserManager<UserProfileModel> users, IDebugService debug)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                var user = await users.GetUserAsync(context.User);
                await debug.Log(e, context.TraceIdentifier, user?.Id);
                if (env.IsDevelopment())
                {
                    throw;
                }
                else
                {
                    context.Response.Redirect($"/Error/500", permanent: false);
                }
            }
        }
    }
}