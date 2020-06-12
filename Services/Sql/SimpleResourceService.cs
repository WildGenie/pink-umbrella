using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql
{
    public class SimpleResourceService : ISimpleResourceService
    {
        private readonly SimpleDbContext _dbContext;

        public SimpleResourceService(SimpleDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<SimpleResourceModel> CreateResource(SimpleResourceModel initial)
        {
            _dbContext.Resources.Add(initial);
            await _dbContext.SaveChangesAsync();
            return initial;
        }

        public async Task DeleteResource(int id, int by_user_id)
        {
            var res = await _dbContext.Resources.FindAsync(id);
            if (res != null) {
                res.WhenDeleted = DateTime.UtcNow;
                res.DeletedByUserId = by_user_id;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<SimpleResourceModel> ForkResource(int id, int userId, int inventoryId)
        {
            var r = new SimpleResourceModel(await _dbContext.Resources.FindAsync(id));
            r.Id = -1;
            r.ForkedFromId = id;
            r.CreatedByUserId = userId;
            r.DeletedByUserId = null;
            r.WhenDeleted = null;
            r.WhenCreated = DateTime.UtcNow;
            r.InventoryId = inventoryId;

            _dbContext.Resources.Add(r);
            await _dbContext.SaveChangesAsync();
            return r;
        }

        public async Task<List<SimpleResourceModel>> GetAllForUser(int id)
        {
            var invs = await _dbContext.Inventories.Where(i => i.OwnerUserId == id).ToListAsync();
            return invs.SelectMany(i => _dbContext.Resources.Where(r => r.InventoryId == i.Id)).ToList();
        }

        public async Task<SimpleResourceModel> GetResource(int id)
        {
            return await _dbContext.Resources.FindAsync(id);
        }

        public async Task<List<SimpleResourceModel>> QueryInventory(int userId, int inventoryId, string text, PaginationModel pagination)
        {
            var forUser = await GetAllForUser(userId);
            return forUser.Where(r => r.Name.Contains(text)).ToList();
        }

        public async Task UpdateAmount(int id, double newAmount)
        {
            var r = await _dbContext.Resources.FindAsync(id);
            r.Amount = newAmount;
            await _dbContext.SaveChangesAsync();
        }
    }
}