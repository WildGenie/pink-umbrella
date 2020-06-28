using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Services.Peer
{
    public interface IPeerConnectionType
    {
        PeerConnectionType ConnectionType { get; }

        IPeerClient Open(PeerModel peer, DbContext context);
    }
}