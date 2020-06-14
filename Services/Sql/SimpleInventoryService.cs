using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using System;

namespace PinkUmbrella.Services.Sql
{
    public class SimpleInventoryService : ISimpleInventoryService
    {
        private readonly SimpleDbContext _dbContext;

        public SimpleInventoryService(SimpleDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<SimpleInventoryModel> CreateInventory(SimpleInventoryModel initial)
        {
            initial.WhenCreated = DateTime.UtcNow;
            _dbContext.Inventories.Add(initial);
            await _dbContext.SaveChangesAsync();
            return initial;
        }

        public async Task<SimpleInventoryModel> Get(int id)
        {
            return await _dbContext.Inventories.FindAsync(id);
        }

        public Task<List<SimpleInventoryModel>> GetForUser(int userId)
        {
            return _dbContext.Inventories.Where(i => i.OwnerUserId == userId).ToListAsync();
        }
    }
}