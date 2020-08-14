using System;
using System.Runtime.Serialization;

namespace Estuary.Util
{
    public class FilterReturnedEmptyResultsException : Exception
    {
        public FilterReturnedEmptyResultsException()
        {
        }

        public FilterReturnedEmptyResultsException(string message) : base(message)
        {
        }

        public FilterReturnedEmptyResultsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FilterReturnedEmptyResultsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}