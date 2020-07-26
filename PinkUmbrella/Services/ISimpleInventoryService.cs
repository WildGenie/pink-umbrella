using System.Threading.Tasks;
using PinkUmbrella.ViewModels.Inventory;
using Tides.Core;

namespace PinkUmbrella.Services
{
    public interface ISimpleInventoryService
    {
        Task<CollectionObject> GetForUser(int userId, int? viewerId);
        Task<BaseObject> Get(int id, int? viewerId);
        Task<BaseObject> CreateInventory(BaseObject initial);
        
        Task<CollectionObject> GetAllLocal();
        
        BaseObject Transform(NewInventoryViewModel inventory);
    }
}