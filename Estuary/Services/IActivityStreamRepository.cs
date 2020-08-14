using System.Threading.Tasks;
using Estuary.Core;

namespace Estuary.Services
{
    public interface IActivityStreamRepository: IHazActivityStreamPipe
    {
        Task<CollectionObject> GetAll(ActivityStreamFilter filter);
        Task<BaseObject> Get(ActivityStreamFilter filter);
        IActivityStreamBox GetBox(ActivityStreamFilter filter);
        Task<BaseObject> Post(string index, ActivityObject item);
        Task<BaseObject> Set(ActivityObject item, ActivityStreamFilter filter);
        Task<BaseObject> Undo(ActivityStreamFilter filter);
    }
}