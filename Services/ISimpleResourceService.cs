using System.Collections.Generic;
using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface ISimpleResourceService
    {
        Task<List<SimpleResourceModel>> GetAllForUser(int id);

        Task<List<SimpleResourceModel>> QueryInventory(int userId, int inventoryId, string text, PaginationModel pagination);

        Task<SimpleResourceModel> GetResource(int id);

        Task<SimpleResourceModel> CreateResource(SimpleResourceModel initial);

        Task<SimpleResourceModel> ForkResource(int id, int userId, int inventoryId);

        Task UpdateAmount(int id, double newAmount);

        Task DeleteResource(int id, int by_user_id);
        
        Task<List<string>> GetBrands();
        
        Task<List<string>> GetCategories();
        
        Task<List<string>> GetUnits();
    }
}