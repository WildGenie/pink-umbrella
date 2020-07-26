using PinkUmbrella.Models;
using Tides.Actors;

namespace PinkUmbrella.Util
{
    public static class ElasticExtensions
    {
        public static ActorObject ToElastic(this UserProfileModel u)
        {
            return new ActorObject {
                objectId = u.Id,
                name = u.DisplayName,
                summary = u.Bio,
                Handle = u.Handle
            };
        }
    }
}