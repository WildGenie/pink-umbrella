using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Services.Peer;
using Tides.Models.Peer;

namespace PinkUmbrella.Services.NoSql
{
    public class PeerConnectionTypeResolver : IPeerConnectionTypeResolver
    {
        private readonly Dictionary<PeerConnectionType, IPeerConnectionType> _types;
        public PeerConnectionTypeResolver(IEnumerable<IPeerConnectionType> types)
        {
            _types = types.ToDictionary(k => k.ConnectionType, v => v);
        }

        public IPeerConnectionType Get(PeerConnectionType connectionType)
                    => _types.TryGetValue(connectionType, out var type) ? type : null;

        public Task<ISyncPeerClient> Open(PeerModel peer, PeerConnectionType connectionType)
                    => Task.FromResult(Get(connectionType)?.Open(peer) as ISyncPeerClient);

        // public Task ResolveMetaData(PeerModel peer, Stream metaJsonStream, PeerConnectionType connectionType)
        // {
        //     if (_types.TryGetValue(connectionType, out var type))
        //     {
        //         type.ResolveMetaData(peer, metaJsonStream);
        //     }
        //     return Task.CompletedTask;
        // }
    }
}