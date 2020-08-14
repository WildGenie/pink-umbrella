using System;
using System.Runtime.Serialization;

namespace Estuary.Exceptions
{
    public class ActivityStreamException : Exception
    {
        public ActivityStreamException()
        {
        }

        public ActivityStreamException(string message) : base(message)
        {
        }

        public ActivityStreamException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ActivityStreamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}