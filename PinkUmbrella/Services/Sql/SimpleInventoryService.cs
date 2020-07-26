using System.Threading.Tasks;
using PinkUmbrella.Repositories;
using System;
using Tides.Core;
using PinkUmbrella.ViewModels.Inventory;

namespace PinkUmbrella.Services.Sql
{
    public class SimpleInventoryService : ISimpleInventoryService
    {
        private readonly SimpleDbContext _dbContext;

        public SimpleInventoryService(SimpleDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<BaseObject> CreateInventory(BaseObject initial)
        {
            initial.published = DateTime.UtcNow;
            // TODO: this won't work
            //_dbContext.Inventories.Add(initial);
            await _dbContext.SaveChangesAsync();
            return initial;
        }

        public async Task<BaseObject> Get(int id, int? viewerId)
        {
            await Task.Delay(1);
            return null;//await _dbContext.Inventories.FindAsync(id);
        }

        public async Task<CollectionObject> GetAllLocal()
        {
            await Task.Delay(1);
            return null;//_dbContext.Inventories.ToListAsync();
        }

        public async Task<CollectionObject> GetForUser(int userId, int? viewerId)
        {
            await Task.Delay(1);
            return null;//(await _dbContext.Inventories.Where(i => i.OwnerUserId == userId).ToListAsync()).Select(Transform).ToList();
        }

        public BaseObject Transform(NewInventoryViewModel inventory)
        {
            throw new NotImplementedException();
        }
    }
}