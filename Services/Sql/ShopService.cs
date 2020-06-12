using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql
{
    public class ShopService : IShopService
    {
        private readonly SimpleDbContext _dbContext;

        public ShopService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ShopModel> GetShopByHandle(string handle)
        {
            return await _dbContext.Shops.SingleOrDefaultAsync(s => s.Handle == handle);
        }

        public Task<ShopModel> GetShopById(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}