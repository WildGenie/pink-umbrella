using seattle.Models;

namespace seattle.Services.Sql
{
    public class FeedService : IFeedService
    {
        public FeedModel GetFeedForUser(int userId, int viewerId, bool includeReplies, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}