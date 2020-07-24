using Tides.Core;

namespace Tides.Objects
{
    public class Relationship : HazObject
    {
        public Relationship(string type = null) : base(type ?? nameof(Relationship)) { }

        public BaseObject subject { get; set; }
        
        public string relationship { get; set; }
    }
}