using System;
using System.IO;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Streams.Json;
using Tides.Models.Public;

namespace Estuary.Streams
{
    public class ReverseObjectIdStreamReader : ObjectIdStreamReader
    {
        public ReverseObjectIdStreamReader(Stream stream, Func<PublicId, Stream> resolver, CustomJsonSerializer serializer):
                base(stream, resolver, serializer)
        {
            _stream.Seek(0, SeekOrigin.End);
        }

        public override async Task<BaseObject> Read()
        {
            var oldPos = _stream.Position;
            if (oldPos > 0 && oldPos % 16 == 0)
            {
                var newPos = _stream.Seek(-16, SeekOrigin.Current);
                if (oldPos == newPos + 16)
                {
                    var ret = await base.Read();
                    _stream.Seek(-16, SeekOrigin.Current);
                    return ret;
                }
            }
            
            return null;
        }
    }
}