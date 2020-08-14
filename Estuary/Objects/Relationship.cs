using Estuary.Core;

namespace Estuary.Objects
{
    public class Relationship : HazObject
    {
        public Relationship() : base(nameof(Relationship), null) { }

        public BaseObject subject { get; set; }
        
        public string relationship { get; set; }
    }
}