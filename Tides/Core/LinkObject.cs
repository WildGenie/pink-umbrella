namespace Tides.Core
{
    public class LinkObject : BaseObject
    {
        public LinkObject(string type = null) : base(type ?? "Link")
        {
        }

        public string href { get; set; }
        public string rel { get; set; }
        public string hreflang { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
}