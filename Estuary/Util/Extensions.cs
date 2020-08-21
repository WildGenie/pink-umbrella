using System;
using System.Collections.Generic;
using System.Linq;
using Estuary.Actors;
using Estuary.Core;
using Tides.Models;

namespace Estuary.Util
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

        public static BaseObject GetPublisher(this ActivityObject thiz) => thiz?.actor?.items?.FirstOrDefault();




        public static ActivityStreamFilter FixType(this ActivityStreamFilter filter, params string[] types)
        {
            filter.types = types;
            return filter;
        }

        public static ActivityStreamFilter FixObjType(this ActivityStreamFilter filter, params string[] types)
        {
            filter.objectTypes = types;
            return filter;
        }

        public static ActivityStreamFilter FixTarget(this ActivityStreamFilter filter, params string[] types)
        {
            filter.targetTypes = types;
            return filter;
        }

        public static ActivityStreamFilter ReactionsOnly(this ActivityStreamFilter filter)
        {
            filter.types = Enum.GetValues(typeof(ReactionType)).Cast<ReactionType>().Select(v => v.ToString()).ToArray();
            return filter;
        }

        
        public static ActorObject ToAudience(this Visibility visibility, ActorObject publisher)
        {
            switch (visibility)
            {
                case Visibility.HIDDEN:
                return publisher;
                case Visibility.VISIBLE_TO_REGISTERED:
                return new Common.Group { Handle = "registered" };
                case Visibility.VISIBLE_TO_FOLLOWERS:
                return new Common.Group { Handle = "followers" };
                case Visibility.VISIBLE_TO_WORLD:
                return null;
                default:
                throw new ArgumentOutOfRangeException();
            }
        }

        public static List<T> IntoNewList<T>(this T first) => new List<T> { first };
    }
}