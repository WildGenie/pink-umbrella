using System.Threading.Tasks;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;

namespace Tides.Services
{
    public interface IActivityStreamRepository
    {
        Task<CollectionObject> GetAll(ActivityStreamFilter filter);
        
        Task<BaseObject> GetActor(ActivityStreamFilter filter);
        Task<CollectionObject> GetActors(ActivityStreamFilter filter);
        Task<CollectionObject> GetFollowers(PublicId publicId, int? viewerId);
        Task<CollectionObject> GetFollowing(PublicId publicId, int? viewerId);

        Task<BaseObject> GetPost(ActivityStreamFilter filter);
        Task<CollectionObject> GetPosts(ActivityStreamFilter filter);
        Task<BaseObject> GetReply(ActivityStreamFilter filter);
        Task<CollectionObject> GetReplies(ActivityStreamFilter filter);
        Task<CollectionObject> GetMentions(ActivityStreamFilter filter);
        Task<BaseObject> GetShop(ActivityStreamFilter filter);
        Task<CollectionObject> GetShops(ActivityStreamFilter filter);
        Task<CollectionObject> GetFollowers(ActivityStreamFilter filter);
        Task<CollectionObject> GetFollowing(ActivityStreamFilter filter);
        Task<BaseObject> GetMedia(ActivityStreamFilter filter);
        Task<BaseObject> GetPeer(ActivityStreamFilter filter);
        Task<CollectionObject> GetInventories(ActivityStreamFilter filter);
        Task<BaseObject> GetInventory(ActivityStreamFilter filter);
        Task<CollectionObject> GetResources(ActivityStreamFilter filter);
        Task<BaseObject> GetResource(ActivityStreamFilter filter);
        Task<CollectionObject> GetPeers(ActivityStreamFilter filter);
        Task<BaseObject> Get();





        Task<BaseObject> Write(BaseObject item);
        
        
        Task<CollectionObject> GetInbox(ActivityStreamFilter filter);
    }
}