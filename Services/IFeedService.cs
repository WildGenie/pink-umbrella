using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface IFeedService
    {
        Task<FeedModel> GetFeedForUser(int userId, int? viewerId, bool includeReplies, PaginationModel pagination);
    }
}