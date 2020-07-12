using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Public;
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
        
        Task<List<PublicProfileModel>> GetProfiles(DateTime? sinceLastUpdated);
        
        Task<List<PostModel>> GetPosts(DateTime? sinceLastUpdated);
        
        Task<List<ShopModel>> GetShops(DateTime? sinceLastUpdated);
        
        Task<List<ArchivedMediaModel>> GetArchivedMedia(DateTime? sinceLastUpdated);
        
        Task<List<PeerModel>> GetPeers(DateTime? sinceLastUpdated);
        
        Task<List<SimpleInventoryModel>> GetInventories(DateTime? sinceLastUpdated);
    }
}