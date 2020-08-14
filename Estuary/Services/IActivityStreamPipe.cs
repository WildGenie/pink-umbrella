using System.Threading.Tasks;
using Estuary.Core;

namespace Estuary.Services
{
    public interface IActivityStreamPipe
    {
         Task<BaseObject> Pipe(ActivityDeliveryContext ctx);
    }
}