using PinkUmbrella.Models;
using PinkUmbrella.Models.Elastic;

namespace PinkUmbrella.Util
{
    public static class ElasticExtensions
    {
        public static ElasticProfileModel ToElastic(this UserProfileModel u)
        {
            return new ElasticProfileModel {
                UserId = u.Id,
                DisplayName = u.DisplayName,
                Bio = u.Bio,
                Handle = u.Handle
            };
        }
    }
}