using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Services
{
    public interface IPeerService
    {
        Task<int> CountAsync();
        Task<List<PeerModel>> GetPeers();
        Task<PeerModel> GetPeer(IPAddressModel address, int? port = null);
        Task AddPeer(PeerModel peer);
        Task RemovePeer(PeerModel peer);
        Task ReplacePeer(PeerModel peer);
        Task PeerAddressChanged(IPAddressModel addressFrom, int portFrom, IPAddressModel addressTo, int portTo);
        Task<IPeerClient> Open(IPAddressModel address, int? port = null);
    }
}