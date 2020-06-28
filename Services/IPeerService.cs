using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Services
{
    public interface IPeerService
    {
        Task<List<PeerModel>> GetPeers();
        Task<PeerModel> GetPeer(string handle);
        Task AddPeer(PeerModel peer);
        Task RemovePeer(PeerModel peer);
        Task ReplacePeer(PeerModel peer);
        Task RenamePeer(string handleFrom, string handleTo);
        Task<IPeerClient> Open(string handle);
    }
}