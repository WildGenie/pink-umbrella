using Estuary.Core;

namespace Estuary.Objects
{
    public class Error : BaseObject
    {
        public Error(string type = null) : base(type ?? nameof(Error), null) { }

        public int errorCode { get; set; }
    }
}