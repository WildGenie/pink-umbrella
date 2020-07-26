using System;
using System.Threading.Tasks;
using Tides.Core;

namespace Tides.Streams
{
    public abstract class ChunkedObjectStreamWriter<TIn, TStream>: BaseObjectStreamWriter where TStream: BaseObjectStreamWriter
    {
        protected TIn _initial, _current;
        protected Func<TIn, TStream> _streamLoader;

        protected long _position = 0;
        protected long _chunkSize = 100;
        protected TStream _stream = null;

        public ChunkedObjectStreamWriter(TIn initial, Func<TIn, TStream> streamLoader)
        {
            _initial = initial;
            _current = initial;
            _streamLoader = streamLoader;
        }
        
        public abstract TIn Progress(TIn current);

        public override bool CanSetPosition => false;

        public override long Position => _position;

        public override Task Write(BaseObject item)
        {
            if (_position % _chunkSize == 0 && _stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            // todo: make sure this doesn't skip first element
            if (_stream == null)
            {
                _current = Progress(_current);
                _stream = _streamLoader(_current);
            }

            if (_stream == null)
            {
                return null;
            }

            _position++;
            return _stream.Write(item);
        }

        public override void SetPosition(long index, bool fromStart = true) => throw new NotImplementedException();

        public override void Dispose() => _stream?.Dispose();
    }
}