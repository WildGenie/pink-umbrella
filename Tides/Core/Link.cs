namespace Tides.Core
{
    public class Link : BaseObject
    {
        public Link(string type = null) : base(type ?? nameof(Link))
        {
        }

        public string href { get; set; }
        public string rel { get; set; }
        public string hreflang { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
}