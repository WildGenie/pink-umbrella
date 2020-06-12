using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface IShopService
    {
        Task<ShopModel> GetShopByHandle(string handle);
        Task<ShopModel> GetShopById(int id);
    }
}