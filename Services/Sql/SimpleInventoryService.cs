using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql
{
    public class SimpleInventoryService : ISimpleInventoryService
    {
        private readonly SimpleDbContext _dbContext;

        public SimpleInventoryService(SimpleDbContext dbContext) {
            _dbContext = dbContext;
        }

        public Task<List<SimpleInventoryModel>> GetForUser(int userId)
        {
            return _dbContext.Inventories.Where(i => i.OwnerUserId == userId).ToListAsync();
        }
    }
}