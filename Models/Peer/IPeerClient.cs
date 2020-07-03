using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PinkUmbrella.Models.Elastic;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Models.Peer
{
    public interface IPeerClient
    {
        IPeerConnectionType Type { get; }
        
        Task<PeerModel> Query();
        
        Task<List<string>> QueryMetaData();
        
        Task<PeerStatsModel> QueryStats();

        Task<string> QueryHtml(string route);

        Task<object> QueryViewModel(string route);
        
        Task<List<ElasticProfileModel>> GetProfiles(DateTime? sinceLastUpdated);
    }
}