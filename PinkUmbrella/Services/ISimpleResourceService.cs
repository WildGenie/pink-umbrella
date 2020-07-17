using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Poncho.Models;
using Poncho.Models.Public;

namespace PinkUmbrella.Services
{
    public interface ISimpleResourceService
    {
        Task<List<SimpleResourceModel>> QueryUser(int userId, int? viewerId, string text, PaginationModel pagination);
        
        Task<List<SimpleResourceModel>> GetAllForUser(int id, int? viewerId);

        Task<List<SimpleResourceModel>> QueryInventory(int inventoryId, int? viewerId, string text, PaginationModel pagination);

        Task<SimpleResourceModel> GetResource(int id, int? viewerId);

        Task<SimpleResourceModel> CreateResource(SimpleResourceModel initial);

        Task<SimpleResourceModel> ForkResource(int id, int userId, int inventoryId);

        Task UpdateAmount(int id, double newAmount);

        Task DeleteResource(int id, int by_user_id);
        
        Task<List<string>> GetBrands();
        
        Task<List<string>> GetCategories();
        
        Task<List<string>> GetUnits();
    }
}