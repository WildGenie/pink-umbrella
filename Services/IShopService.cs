using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface IShopService
    {
        Task<ShopModel> GetShopByHandle(string handle);
        Task<ShopModel> GetShopById(int id);
    }
}