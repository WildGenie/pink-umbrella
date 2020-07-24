using Tides.Core;

namespace Tides.Objects
{
    public class Place : BaseObject
    {
        public Place(string type = null) : base(type ?? nameof(Place)) { }

        public double accuracy { get; set; }
        public double altitude { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double radius { get; set; }
        public string units  { get; set; } //  	"cm" | " feet" | " inches" | " km" | " m" | " miles" 
    }
}