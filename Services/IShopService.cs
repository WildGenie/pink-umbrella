using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IShopService
    {
        Task BindReferences(ShopModel shop, int? viewerId);

        bool CanView(ShopModel shop, int? viewerId);

        Task<ShopModel> GetShopByHandle(string handle, int? viewerId);
        
        Task<ShopModel> GetShopById(PublicId id, int? viewerId);

        Task<PaginatedModel<ShopModel>> GetMostReportedShops();

        Task<PaginatedModel<ShopModel>> GetMostDislikedShops();

        Task<List<ShopModel>> GetShopsForUser(PublicId userId, int? viewerId);

        Task<List<ShopModel>> GetAllShops(int? viewerId);

        Task DeleteShop(int id, int userId);

        Task<ArgumentException> TryCreateShop(ShopModel shop);

        Task<List<ShopModel>> GetShopsTaggedUnder(TagModel tag, int? viewerId);

        Task<bool> HandleExists(string handle);
        
        Task<List<ShopModel>> GetAllLocal();
    }
}