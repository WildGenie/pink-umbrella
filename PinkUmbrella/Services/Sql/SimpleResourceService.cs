using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Poncho.Models;
using Poncho.Models.Public;

namespace PinkUmbrella.Services.Sql
{
    public class SimpleResourceService : ISimpleResourceService
    {
        private readonly SimpleDbContext _dbContext;

        public SimpleResourceService(SimpleDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<SimpleResourceModel> CreateResource(SimpleResourceModel initial)
        {
            initial.WhenCreated = DateTime.UtcNow;
            initial.Units ??= string.Empty;
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

        public async Task<List<SimpleResourceModel>> GetAllForUser(int id, int? viewerId)
        {
            var invs = await _dbContext.Inventories.Where(i => i.OwnerUserId == id).ToListAsync();
            return invs.SelectMany(i => _dbContext.Resources.Where(r => r.InventoryId == i.Id)).ToList();
        }

        public Task<List<SimpleResourceModel>> GetAllForInventory(int id, int? viewerId)
        {
            return _dbContext.Resources.Where(r => r.InventoryId == id).ToListAsync();
        }

        public Task<List<string>> GetBrands()
        {
            return _dbContext.Resources.Select(r => r.Brand).Distinct().ToListAsync();
        }

        public Task<List<string>> GetCategories()
        {
            return _dbContext.Resources.Select(r => r.Category).Distinct().ToListAsync();
        }

        public async Task<SimpleResourceModel> GetResource(int id, int? viewerId)
        {
            return await _dbContext.Resources.FindAsync(id);
        }

        public Task<List<string>> GetUnits()
        {
            return _dbContext.Resources.Select(r => r.Units).Distinct().ToListAsync();
        }

        public async Task<List<SimpleResourceModel>> QueryInventory(int inventoryId, int? viewerId, string text, PaginationModel pagination)
        {
            var forUser = await GetAllForInventory(inventoryId, viewerId);
            if (string.IsNullOrWhiteSpace(text)) {
                return forUser;
            } else {
                return forUser.Where(r => r.Name.Contains(text)).ToList();
            }
        }

        public async Task<List<SimpleResourceModel>> QueryUser(int userId, int? viewerId, string text, PaginationModel pagination)
        {
            var forUser = await GetAllForUser(userId, viewerId);
            if (string.IsNullOrWhiteSpace(text)) {
                return forUser;
            } else {
                return forUser.Where(r => r.Name.Contains(text)).ToList();
            }
        }

        public async Task UpdateAmount(int id, double newAmount)
        {
            var r = await _dbContext.Resources.FindAsync(id);
            r.Amount = newAmount;
            await _dbContext.SaveChangesAsync();
        }
    }
}