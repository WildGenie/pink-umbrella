using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Services.Peer
{
    public interface IPeerConnectionTypeResolver
    {
        Task<IPeerClient> Open(PeerModel peer, DbContext context, PeerConnectionType connectionType);
        
        IPeerConnectionType Get(PeerConnectionType connectionType);
    }
}