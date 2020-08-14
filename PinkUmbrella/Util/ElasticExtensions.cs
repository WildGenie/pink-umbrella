using PinkUmbrella.Models;
using Estuary.Actors;
using static Estuary.Actors.Common;

namespace PinkUmbrella.Util
{
    public static class ElasticExtensions
    {
        public static ActorObject ToElastic(this UserProfileModel u)
        {
            return new Person
            {
                objectId = u.Id,
                name = u.DisplayName,
                summary = u.Bio,
                Handle = u.Handle
            };
        }
    }
}