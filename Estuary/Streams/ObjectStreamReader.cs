using System;
using System.IO;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Streams.Json;

namespace Estuary.Streams
{
    public class ObjectStreamReader : BaseObjectStreamReader
    {
        private long _position = 0;
        private readonly StreamReader _reader;
        private readonly CustomJsonSerializer _serializer;

        public ObjectStreamReader(Stream stream, CustomJsonSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _reader = serializer.OpenReader(stream ?? throw new ArgumentNullException(nameof(stream)));
        }

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override Task<BaseObject> Read() => _serializer.Deserialize(_reader);

        public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();

        public override void Dispose() => _reader.Dispose();
    }
}