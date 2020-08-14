using System;
using System.Threading.Tasks;
using Estuary.Core;

namespace PinkUmbrella.Services
{
    public interface IShopService
    {
        // Task<BaseObject> GetShopByHandle(string handle, int? viewerId);
        
        // Task<BaseObject> GetShopById(PublicId id, int? viewerId);

        // Task<CollectionObject> GetMostReportedShops();

        // Task<CollectionObject> GetMostDislikedShops();

        // Task<CollectionObject> GetShopsForUser(PublicId userId, int? viewerId);

        // Task<CollectionObject> GetAllShops(int? viewerId);

        // Task DeleteShop(int id, int userId);

        Task<BaseObject> TryCreateShop(BaseObject shop);

        Task<BaseObject> GetShopsTaggedUnder(BaseObject tag, int? viewerId);
        
        Task<BaseObject> GetAllLocal();
    }
}