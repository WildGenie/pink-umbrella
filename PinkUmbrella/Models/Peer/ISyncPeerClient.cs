using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Public;
using Tides.Models.Auth;
using Tides.Models.Peer;

namespace PinkUmbrella.Models.Peer
{
    public interface ISyncPeerClient: IPeerClient
    {
        
        Task<List<PublicProfileModel>> GetProfiles(DateTime? sinceLastUpdated, KeyPair keys);
        
        Task<List<PostModel>> GetPosts(DateTime? sinceLastUpdated, KeyPair keys);
        
        Task<List<ShopModel>> GetShops(DateTime? sinceLastUpdated, KeyPair keys);
        
        Task<List<ArchivedMediaModel>> GetArchivedMedia(DateTime? sinceLastUpdated, KeyPair keys);
        
        Task<List<PeerModel>> GetPeers(DateTime? sinceLastUpdated, KeyPair keys);
        
        Task<List<SimpleInventoryModel>> GetInventories(DateTime? sinceLastUpdated, KeyPair keys);
    }
}