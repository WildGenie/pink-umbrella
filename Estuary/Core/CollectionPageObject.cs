namespace Estuary.Core
{
    public class CollectionPageObject: CollectionObject
    {
        public CollectionPageObject(string type = null) : base(type ?? "CollectionPage", "Collection")
        {
        }

        public BaseObject partOf { get; set; }
        public BaseObject next { get; set; }
        public BaseObject prev { get; set; }
    }
}