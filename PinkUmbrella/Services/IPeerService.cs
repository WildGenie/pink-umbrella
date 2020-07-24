using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Peer;
using Tides.Models.Auth;
using Tides.Models.Peer;

namespace PinkUmbrella.Services
{
    public interface IPeerService
    {
        Task<int> CountAsync();
        Task<List<PeerModel>> GetPeers();
        Task<PeerModel> GetPeer(SavedIPAddressModel address, int? port = null);
        Task AddPeer(PeerModel peer);
        Task RemovePeer(PeerModel peer);
        Task ReplacePeer(PeerModel peer);
        Task PeerAddressChanged(SavedIPAddressModel addressFrom, int portFrom, SavedIPAddressModel addressTo, int portTo);
        Task<ISyncPeerClient> Open(IPAddressModel address, int? port = null);
    }
}