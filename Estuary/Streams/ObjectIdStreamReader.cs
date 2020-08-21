using System;
using System.IO;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Objects;
using Estuary.Streams.Json;
using Tides.Models.Public;
using static Estuary.Objects.Common;

namespace Estuary.Streams
{
    public class ObjectIdStreamReader : BaseObjectStreamReader
    {
        private long _position = 0;
        protected readonly Stream _stream;
        private readonly Func<PublicId, Stream> _resolver;
        private readonly CustomJsonSerializer _serializer;

        public ObjectIdStreamReader(Stream stream, Func<PublicId, Stream> resolver, CustomJsonSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override async Task<BaseObject> Read()
        {
            var guidBytes = new byte[16];
            var num = await _stream.ReadAsync(guidBytes);
            if (num < guidBytes.Length)
            {
                return new Error { statusCode = 404, errorCode = 404, summary = "Guid size mismatch" };
            }

            var s = _resolver.Invoke(new PublicId(new Guid(guidBytes)));
            if (s != null)
            {
                BaseObject ret;
                using (var reader = _serializer.OpenReader(s))
                {
                    ret = await _serializer.Deserialize(reader);
                }
                if (ret != null)
                {
                    return ret;
                }
            }
            return new Error { statusCode = 404, errorCode = 404, summary = "Object not found" };
        }

        public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();

        public override void Dispose() => _stream.Dispose();



        public class Empty : BaseObjectStreamReader
        {
            public override bool CanSetPosition => true;

            public override long Position => 0;

            public override void Dispose() { }

            public override Task<BaseObject> Read() => Task.FromResult<BaseObject>(new Error { content = "This stream is empty or does not exist", errorCode = 404 });

            public override void SetPosition(long index, bool fromStart = true) => throw new System.NotImplementedException();
        }
    }
}