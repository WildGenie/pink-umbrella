using System.Collections.Generic;

namespace Estuary.Core
{
    public class OrderedCollectionObject: CollectionObject
    {
        public OrderedCollectionObject(string type) : base(type ?? "OrderedCollection", "Collection")
        {
        }

        public OrderedCollectionObject() : this(null) {}

        public List<BaseObject> orderedItems { get; set; }
    }
}