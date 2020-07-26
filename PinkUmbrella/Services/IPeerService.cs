using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Auth;
using Tides.Actors;
using Tides.Models.Auth;
using Tides.Services;

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