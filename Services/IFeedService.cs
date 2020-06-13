using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface IFeedService
    {
        Task<FeedModel> GetFeedForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination);
        Task<FeedModel> GetMentionsForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination);
        Task<FeedModel> GetPostsForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination);
    }
}