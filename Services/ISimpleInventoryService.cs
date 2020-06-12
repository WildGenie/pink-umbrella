using System.Collections.Generic;
using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface ISimpleInventoryService
    {
        Task<List<SimpleInventoryModel>> GetForUser(int userId);
        Task<SimpleInventoryModel> Get(int id);
        Task<SimpleInventoryModel> CreateInventory(SimpleInventoryModel initial);
    }
}