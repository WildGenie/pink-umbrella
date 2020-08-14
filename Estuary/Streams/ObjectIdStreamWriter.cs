using System;
using System.IO;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;

namespace Estuary.Streams
{
    public class ObjectIdStreamWriter : BaseObjectStreamWriter
    {
        private long _position = 0;
        private readonly Stream _stream;

        public ObjectIdStreamWriter(Stream stream, IActivityStreamRepository ctx)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override async Task Write(BaseObject item)
        {
            await _stream.WriteAsync(item.PublicId.AsGuid().ToByteArray());
            await _stream.FlushAsync();
            _position++;
        }

        public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();

        public override void Dispose() => _stream.Dispose();
    }
}