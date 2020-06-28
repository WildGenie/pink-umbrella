using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PinkUmbrella.Util
{
    public class ExternalDbOptions
    {
        public static Task<string> ExtractDomain(HttpContext context) {
            var host = context.Request.Host.Host;
            var subdomain = string.IsNullOrWhiteSpace(host) ? string.Empty : host.Split('.').First();
            return Task.FromResult(subdomain);
        }

        public Func<string, Task<DbContext>> OpenDbContext { get; set; }

        public Func<HttpContext, Task<string>> ExtractDbHandle { get; set; }
    }
}