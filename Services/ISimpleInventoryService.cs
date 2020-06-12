using System.Collections.Generic;
using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface ISimpleInventoryService
    {
        Task<List<SimpleInventoryModel>> GetForUser(int userId);
    }
}