using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Peer;

namespace PinkUmbrella.Services.Peer
{
    public class RESTPeerClientType : IPeerConnectionType
    {
        public PeerConnectionType ConnectionType { get; } = PeerConnectionType.RestApiV1;

        public IPeerClient Open(PeerModel peer, DbContext context) => new RESTPeerClient(this, peer, context);
    }
}