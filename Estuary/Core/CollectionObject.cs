using System.Collections.Generic;

namespace Estuary.Core
{
    public class CollectionObject: BaseObject
    {
        public CollectionObject(string type, string baseType) : base(type ?? "Collection", baseType)
        {
        }

        public CollectionObject(): this(null, null) {}

        public int totalItems { get; set; }
        public BaseObject current { get; set; }
        public BaseObject first { get; set; }
        public BaseObject last { get; set; }
        public List<BaseObject> items { get; set; }

        public void Add(BaseObject item) => items.Add(item);


        public override bool IsDefined => base.IsDefined || totalItems > 0 || (items != null && items.Count > 0) ||
                (first != null && first.IsDefined) || (last != null && last.IsDefined) || (current != null && current.IsDefined);
    }
}