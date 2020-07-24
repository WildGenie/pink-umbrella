using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;
using Tides.Models.Peer;

namespace PinkUmbrella.Services
{
    public interface IElasticService
    {
        Task SyncProfiles(long peerId, List<PublicProfileModel> profiles);
        Task SyncProfile(long peerId, PublicProfileModel profile);
        
        
        Task SyncPosts(long peerId, List<PostModel> items);
        Task SyncPost(long peerId, PostModel item);
        
        
        Task SyncShops(long peerId, List<ShopModel> items);
        Task SyncShop(long peerId, ShopModel item);
        
        
        Task SyncArchivedMedias(long peerId, List<ArchivedMediaModel> items);
        Task SyncArchivedMedia(long peerId, ArchivedMediaModel items);
        
        
        Task SyncInventories(long peerId, List<SimpleInventoryModel> items);
        Task SyncInventory(long peerId, SimpleInventoryModel items);
        
        
        Task SyncPeers(long peerId, List<PeerModel> items);
    }
}