using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface ISimpleInventoryService
    {
        Task<List<SimpleInventoryModel>> GetForUser(int userId, int? viewerId);
        Task<SimpleInventoryModel> Get(int id, int? viewerId);
        Task<SimpleInventoryModel> CreateInventory(SimpleInventoryModel initial);
    }
}