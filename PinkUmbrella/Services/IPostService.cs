using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IPostService
    {
        Task<BaseObject> TryCreateTextPost(ActorObject publisher, string content, Visibility visibility);

        Task<BaseObject> GetMentionsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination);
        
        Task<BaseObject> GetPostsForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination);
    }
}