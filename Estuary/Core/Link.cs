namespace Estuary.Core
{
    public class Link : BaseObject
    {
        public Link(string type, string baseType) : base(type ?? nameof(Link), baseType)
        {
        }

        public Link(): this(null, null) {}

        public string href { get; set; }
        public string rel { get; set; }
        public string hreflang { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
}