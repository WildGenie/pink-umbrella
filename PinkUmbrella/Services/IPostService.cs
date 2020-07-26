using System.Threading.Tasks;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IPostService
    {
        Task<BaseObject> TryCreateTextPost(int userId, string content, Visibility visibility);

        Task<CollectionObject> GetMentionsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination);
        
        Task<CollectionObject> GetPostsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination);
    }
}