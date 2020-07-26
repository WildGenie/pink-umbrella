using System.Threading.Tasks;
using Tides.Core;

namespace PinkUmbrella.Services
{
    public interface IElasticService
    {
        Task SyncObjects(long peerId, CollectionObject items);
        Task SyncObject(long peerId, BaseObject item);

        Task SyncProfiles(long peerId, CollectionObject profiles);
        Task SyncProfile(long peerId, BaseObject profile);
        
        
        Task SyncPosts(long peerId, CollectionObject items);
        Task SyncPost(long peerId, BaseObject item);
        
        
        Task SyncShops(long peerId, CollectionObject items);
        Task SyncShop(long peerId, BaseObject item);
        
        
        Task SyncArchivedMedias(long peerId, CollectionObject items);
        Task SyncArchivedMedia(long peerId, BaseObject items);
        
        
        Task SyncInventories(long peerId, CollectionObject items);
        Task SyncInventory(long peerId, BaseObject items);
        
        
        Task SyncPeers(long peerId, CollectionObject items);
        Task SyncPeer(long peerId, BaseObject items);
    }
}