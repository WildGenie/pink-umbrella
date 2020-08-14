using Estuary.Core;

namespace Estuary.Objects
{
    public class Place : BaseObject
    {
        public Place() : base(nameof(Place), null) { }

        public double accuracy { get; set; }
        public double altitude { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double radius { get; set; }
        public string units  { get; set; } //  	"cm" | " feet" | " inches" | " km" | " m" | " miles" 
    }
}