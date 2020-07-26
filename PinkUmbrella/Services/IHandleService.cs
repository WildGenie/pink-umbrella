using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface IHandleService
    {
        Task<ObjectHandleModel> GetByHandle(string handle);
        
        Task<ObjectHandleModel> GetByIdAndType(int id, string type);

        Task<bool> HandleExists(string handle);
        
        Task<List<ObjectHandleModel>> GetCompletionsFor(string prefix);
    }
}