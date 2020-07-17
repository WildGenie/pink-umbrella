using System.Collections.Generic;
using System.Threading.Tasks;
using Poncho.Models.Auth;

namespace Poncho.Models.Peer
{
    public interface IPeerClient
    {
        IPeerConnectionType Type { get; }
        
        Task<PeerModel> Query(KeyPair keys);
        
        Task<List<string>> QueryMetaData(KeyPair keys);
        
        Task<PeerStatsModel> QueryStats(KeyPair keys);

        Task<object> QueryViewModel(string route, KeyPair keys);

        Task<string> QueryHtml(string route, KeyPair keys);

        Task<string> QueryJson(string route, KeyPair keys);
    }
}