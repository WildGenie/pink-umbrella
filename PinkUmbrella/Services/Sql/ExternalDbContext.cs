using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Util;
using Poncho.Models.Peer;

namespace PinkUmbrella.Services.Sql
{
    public class ExternalDbContext : IExternalDbContext
    {
        private readonly ExternalDbOptions _options;
        
        public ExternalDbContext(ExternalDbOptions options)
        {
            _options = options;
        }

        public DbContext Context { get; set; }
        
        public PeerModel Peer { get; set; }

        public async Task SwitchTo(string peerHandle)
        {
            if (Context != null)
            {
                await Context.DisposeAsync();
                Context = null;
            }

            if (!string.IsNullOrWhiteSpace(peerHandle))
            {
                Context = await _options.OpenDbContext(peerHandle);
            }
        }

        public T Get<T>() where T : DbContext => Context as T;

        public void Dispose() => this.Context?.Dispose();
    }
}