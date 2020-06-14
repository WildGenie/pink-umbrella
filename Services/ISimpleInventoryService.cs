using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface ISimpleInventoryService
    {
        Task<List<SimpleInventoryModel>> GetForUser(int userId);
        Task<SimpleInventoryModel> Get(int id);
        Task<SimpleInventoryModel> CreateInventory(SimpleInventoryModel initial);
    }
}