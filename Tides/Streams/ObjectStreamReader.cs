using System.IO;
using System.Threading.Tasks;
using Tides.Core;

namespace Tides.Streams
{
    public class ObjectStreamReader : BaseObjectStreamReader
    {
        private long _position = 0;
        private readonly Stream _stream;

        public ObjectStreamReader(Stream stream)
        {
            _stream = stream;
        }

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override async Task<BaseObject> Read()
        {
            var start = _stream.Position;
            var peek = new byte[256];
            var numRead = await _stream.ReadAsync(peek);
            if (numRead > 0)
            {
                _stream.Position = start;
                _position++;
                return (BaseObject) await System.Text.Json.JsonSerializer.DeserializeAsync(_stream, ResolveType(peek));
            }
            else
            {
                return null;
            }
        }

        public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();

        public override void Dispose() => _stream.Dispose();
    }
}