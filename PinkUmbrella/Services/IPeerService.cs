using System.Collections.Generic;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Services;
using PinkUmbrella.Models.Auth;
using Tides.Models.Auth;

namespace PinkUmbrella.Services
{
    public interface IPeerService
    {
        Task<int> CountAsync();
        Task<List<Peer>> GetPeers();
        Task<Peer> GetPeer(SavedIPAddressModel address, int? port = null);
        Task AddPeer(Peer peer);
        Task RemovePeer(Peer peer);
        Task ReplacePeer(Peer peer);
        Task PeerAddressChanged(SavedIPAddressModel addressFrom, int portFrom, SavedIPAddressModel addressTo, int portTo);
        Task<IActivityStreamRepository> Open(IPAddressModel address, int? port = null);
    }
}