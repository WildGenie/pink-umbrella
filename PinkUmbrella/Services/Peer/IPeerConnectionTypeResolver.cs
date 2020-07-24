using System.Threading.Tasks;
using PinkUmbrella.Models.Peer;
using Tides.Models.Peer;

namespace PinkUmbrella.Services.Peer
{
    public interface IPeerConnectionTypeResolver
    {
        Task<ISyncPeerClient> Open(PeerModel peer, PeerConnectionType connectionType);
        
        IPeerConnectionType Get(PeerConnectionType connectionType);
    }
}