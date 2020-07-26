using System.Threading.Tasks;
using Tides.Core;

namespace Tides.Services
{
    public interface IActivityStreamPipe
    {
         Task<BaseObject> Pipe(BaseObject value, bool isReading);
    }
}