using System.Collections.Generic;

namespace Tides.Core
{
    public class OrderedCollectionObject: CollectionObject
    {
        public OrderedCollectionObject(string type = null) : base(type ?? "OrderedCollection")
        {
        }

        public List<BaseObject> orderedItems { get; set; }
    }
}