using System.Collections.Generic;
using System.Linq;
using Tides.Core;

namespace Tides.Util
{
    public static class Extensions
    {
        public static CollectionObject ToCollection(this IEnumerable<BaseObject> thiz)
        {
            return new CollectionObject
            {
                items = thiz.ToList()
            };
        }
        public static OrderedCollectionObject ToOrderedCollection(this IEnumerable<BaseObject> thiz)
        {
            return new OrderedCollectionObject
            {
                items = thiz.ToList()
            };
        }
    }
}