using Estuary.Core;

namespace Estuary.Services
{
    public interface IActivityStreamBoxProvider
    {
         IActivityStreamBox Resolve(ActivityStreamFilter filter, IActivityStreamRepository ctx);
    }
}