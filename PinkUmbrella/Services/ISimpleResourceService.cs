using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Estuary.Core;
using Tides.Models;

namespace PinkUmbrella.Services
{
    public interface ISimpleResourceService
    {
        Task<CollectionObject> QueryUser(int userId, int? viewerId, string text, PaginationModel pagination);
        
        Task<CollectionObject> GetAllForUser(int id, int? viewerId);

        Task<CollectionObject> QueryInventory(int inventoryId, int? viewerId, string text, PaginationModel pagination);

        Task<BaseObject> GetResource(int id, int? viewerId);

        Task<BaseObject> CreateResource(BaseObject initial);

        Task<BaseObject> ForkResource(int id, int userId, int inventoryId);

        Task<BaseObject> Transform(SimpleResourceModel resource);

        Task UpdateAmount(int id, double newAmount);

        Task DeleteResource(int id, int by_user_id);
        
        Task<List<string>> GetBrands();
        
        Task<List<string>> GetCategories();
        
        Task<List<string>> GetUnits();
    }
}