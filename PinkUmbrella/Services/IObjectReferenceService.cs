using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Estuary.Core;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IObjectReferenceService
    {
        Task<ObjectContentModel> GetByHandle(string handle, string type);
        
        Task<ObjectContentModel> GetById(int id, string type);

        Task<bool> HandleExists(string handle, string type);
        
        Task<List<ObjectContentModel>> GetCompletionsFor(string prefix, string type);
        
        Task<ObjectContentModel> UploadObject(PublicId publisherId, BaseObject obj);
    }
}