using System.Threading.Tasks;
using Estuary.Core;

namespace Estuary.Services
{
    public interface IActivityStreamBox
    {
        ActivityStreamFilter filter { get; }

        IActivityStreamRepository ctx { get; }

        Task<CollectionObject> Get(ActivityStreamFilter filter);
        
        Task<BaseObject> Write(BaseObject item);
    }
}