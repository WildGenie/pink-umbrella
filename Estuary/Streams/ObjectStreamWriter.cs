using System;
using System.IO;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;
using Estuary.Streams.Json;

namespace Estuary.Streams
{
    public class ObjectStreamWriter : BaseObjectStreamWriter
    {
        private long _position = 0;
        private readonly Stream _stream;
        private readonly CustomJsonSerializer _serializer;
        private readonly IActivityStreamRepository _ctx;

        public ObjectStreamWriter(Stream stream, CustomJsonSerializer serializer, IActivityStreamRepository ctx)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _ctx = ctx;
        }

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override async Task Write(BaseObject item)
        {
            await _serializer.Serialize(item, _stream);
            _stream.WriteByte(10);
            await _stream.FlushAsync();
            _position++;
        }

        public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();

        public override void Dispose() => _stream.Dispose();
    }
}