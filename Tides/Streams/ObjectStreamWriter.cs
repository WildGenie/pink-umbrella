using System.IO;
using System.Threading.Tasks;
using Tides.Core;

namespace Tides.Streams
{
    public class ObjectStreamWriter : BaseObjectStreamWriter
    {
        private long _position = 0;
        private readonly Stream _stream;

        public ObjectStreamWriter(Stream stream)
        {
            _stream = stream;
        }

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override async Task Write(BaseObject item)
        {
            await System.Text.Json.JsonSerializer.SerializeAsync(_stream, item);
            _position++;
        }

        public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();

        public override void Dispose() => _stream.Dispose();
    }
}