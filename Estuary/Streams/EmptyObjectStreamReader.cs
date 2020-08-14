using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Objects;

namespace Estuary.Streams
{
    public class EmptyObjectStreamReader : BaseObjectStreamReader
    {
        public override bool CanSetPosition => true;

        public override long Position => 0;

        public override void Dispose() { }

        public override Task<BaseObject> Read() => Task.FromResult<BaseObject>(new Error { content = "This stream is empty or does not exist", errorCode = 404 });

        public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();
    }
}