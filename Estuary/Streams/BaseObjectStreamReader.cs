using System.Threading.Tasks;
using Estuary.Core;

namespace Estuary.Streams
{
    public abstract class BaseObjectStreamReader: BaseObjectStream
    {
        public abstract Task<BaseObject> Read();
    }
}