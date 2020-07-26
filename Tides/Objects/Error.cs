using Tides.Core;

namespace Tides.Objects
{
    public class Error : BaseObject
    {
        public Error(string type = null) : base(type ?? nameof(Error)) { }

        public int errorCode { get; set; }
    }
}