using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;
using Poncho.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IPublicProfileService
    {
        Task<PublicProfileModel> GetUser(PublicId id, int? viewerId);

        Task<PublicProfileModel> GetUser(string handle, int? viewerId);

        Task BindReferences(PublicProfileModel user, int? viewerId);

        bool CanView(PublicProfileModel user, int? viewerId);
        
        Task<PublicProfileModel> Transform(UserProfileModel privateModel, long peerId, int? viewerId);
        
        Task<PublicProfileModel[]> GetFollowers(PublicId id, int? viewerId);
        
        Task<PublicProfileModel[]> GetFollowing(PublicId id, int? viewerId);

        Task<List<PublicProfileModel>> GetAllLocal();
    }
}