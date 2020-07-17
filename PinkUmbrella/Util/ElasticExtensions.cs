using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;

namespace PinkUmbrella.Util
{
    public static class ElasticExtensions
    {
        public static PublicProfileModel ToElastic(this UserProfileModel u)
        {
            return new PublicProfileModel {
                UserId = u.Id,
                DisplayName = u.DisplayName,
                Bio = u.Bio,
                Handle = u.Handle
            };
        }
    }
}