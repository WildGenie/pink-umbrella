using System;

namespace Tides.Streams
{
    public abstract class BaseObjectStream: IDisposable
    {
        public abstract void SetPosition(long index, bool fromStart = true);
        
        public abstract void Dispose();

        public abstract bool CanSetPosition { get; }

        public abstract long Position { get; }
    }
}