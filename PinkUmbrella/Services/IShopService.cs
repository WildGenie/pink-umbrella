using System;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Tides.Core;
using Tides.Models.Public;

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

        Task<ArgumentException> TryCreateShop(BaseObject shop);

        Task<CollectionObject> GetShopsTaggedUnder(BaseObject tag, int? viewerId);
        
        // Task<CollectionObject> GetAllLocal();
    }
}