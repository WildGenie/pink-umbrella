using System.Collections.Generic;

namespace Tides.Core
{
    public class CollectionObject: BaseObject
    {
        public CollectionObject(string type = null) : base(type ?? "Collection")
        {
        }

        public int totalItems { get; set; }
        public BaseObject current { get; set; }
        public BaseObject first { get; set; }
        public BaseObject last { get; set; }
        public List<BaseObject> items { get; set; }
    }
}