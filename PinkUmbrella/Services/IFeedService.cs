using System.Threading.Tasks;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IFeedService
    {
        Task<CollectionObject> GetFeedForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination);
    }
}