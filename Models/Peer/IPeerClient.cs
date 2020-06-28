using System.IO;
using System.Threading.Tasks;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Models.Peer
{
    public interface IPeerClient
    {
        IPeerConnectionType Type { get; }
        
        Task<PeerModel> Query();
        
        Task<PeerModelMetaData> QueryMetaData();
    }
}