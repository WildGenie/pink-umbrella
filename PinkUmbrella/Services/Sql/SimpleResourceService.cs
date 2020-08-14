using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Repositories;
using Tides.Models;
using Estuary.Core;

namespace PinkUmbrella.Services.Sql
{
    public class SimpleResourceService : ISimpleResourceService
    {
        private readonly SimpleDbContext _dbContext;

        public SimpleResourceService(SimpleDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<BaseObject> CreateResource(BaseObject initial)
        {
            initial.published = DateTime.UtcNow;
            // initial.units ??= string.Empty;
            // _dbContext.Resources.Add(initial);
            await _dbContext.SaveChangesAsync();
            return initial;
        }

        public async Task DeleteResource(int id, int by_user_id)
        {
            var res = await _dbContext.Resources.FindAsync(id);
            if (res != null) {
                // res.WhenDeleted = DateTime.UtcNow;
                // res.DeletedByUserId = by_user_id;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<BaseObject> ForkResource(int id, int userId, int inventoryId)
        {
            // var r = new BaseObject(await _dbContext.Resources.FindAsync(id));
            // r.Id = -1;
            // r.ForkedFromId = id;
            // r.CreatedByUserId = userId;
            // r.DeletedByUserId = null;
            // r.WhenDeleted = null;
            // r.WhenCreated = DateTime.UtcNow;
            // r.InventoryId = inventoryId;

            // _dbContext.Resources.Add(r);
            // await _dbContext.SaveChangesAsync();
            // return r;
            await Task.Delay(1);
            return null;
        }

        public async Task<CollectionObject> GetAllForUser(int id, int? viewerId)
        {
            //var invs = await _dbContext.Inventories.Where(i => i.OwnerUserId == id).ToListAsync();
            //return invs.SelectMany(i => _dbContext.Resources.Where(r => r.InventoryId == i.Id)).ToList();
            await Task.Delay(1);
            return null;
        }

        public async Task<CollectionObject> GetAllForInventory(int id, int? viewerId)
        {
            //return _dbContext.Resources.Where(r => r.InventoryId == id).ToListAsync();
            await Task.Delay(1);
            return null;
        }

        public Task<List<string>> GetBrands()
        {
            return _dbContext.Resources.Select(r => r.Brand).Distinct().ToListAsync();
        }

        public Task<List<string>> GetCategories()
        {
            return _dbContext.Resources.Select(r => r.Category).Distinct().ToListAsync();
        }

        public async Task<BaseObject> GetResource(int id, int? viewerId)
        {
            // return await _dbContext.Resources.FindAsync(id);
            await Task.Delay(1);
            return null;
        }

        public Task<List<string>> GetUnits()
        {
            return _dbContext.Resources.Select(r => r.Units).Distinct().ToListAsync();
        }

        public async Task<CollectionObject> QueryInventory(int inventoryId, int? viewerId, string text, PaginationModel pagination)
        {
            var forUser = await GetAllForInventory(inventoryId, viewerId);
            if (!string.IsNullOrWhiteSpace(text)) {
                forUser.items = forUser.items.Where(r => r.name.Contains(text)).ToList();
            }
            return forUser;
        }

        public async Task<CollectionObject> QueryUser(int userId, int? viewerId, string text, PaginationModel pagination)
        {
            var forUser = await GetAllForUser(userId, viewerId);
            if (!string.IsNullOrWhiteSpace(text)) {
                forUser.items = forUser.items.Where(r => r.name.Contains(text)).ToList();
            }
            return forUser;
        }

        public async Task UpdateAmount(int id, double newAmount)
        {
            var r = await _dbContext.Resources.FindAsync(id);
            r.Amount = newAmount;
            await _dbContext.SaveChangesAsync();
        }

        public Task<BaseObject> Transform(Models.SimpleResourceModel resource)
        {
            throw new NotImplementedException();
        }
    }
}