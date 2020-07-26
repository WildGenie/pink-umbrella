using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tides.Core;

namespace Tides.Streams
{
    public class ChunkedObjectStreamReader<TIn, TStream>: BaseObjectStreamReader where TStream: BaseObjectStreamReader
    {
        protected IEnumerator<TIn> _chunks;

        protected Func<TIn, TStream> _streamLoader;

        protected long _position = 0;
        protected TStream _stream = null;

        public ChunkedObjectStreamReader(IEnumerable<TIn> chunks, Func<TIn, TStream> streamLoader)
        {
            _chunks = chunks.GetEnumerator();
            _streamLoader = streamLoader;
        }

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override Task<BaseObject> Read()
        {
            // todo: make sure this doesn't skip first element
            if (_stream == null)
            {
                if (_chunks.MoveNext())
                {
                    _stream = _streamLoader(_chunks.Current);
                }
            }

            if (_stream == null)
            {
                return null;
            }

            _position++;
            return _stream.Read();
        }

        public override void SetPosition(long index, bool fromStart = true) => _stream?.Dispose();
    }
}