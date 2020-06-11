using seattle.Models;

namespace seattle.Services
{
    public interface IFeedService
    {
        FeedModel GetFeedForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination);
    }
}