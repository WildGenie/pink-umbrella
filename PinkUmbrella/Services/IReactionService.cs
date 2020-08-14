using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IReactionService
    {
        Task<string> React(int userId, PublicId toId, ReactionType type);
        Task UnReact(int userId, PublicId toId, ReactionType type);
        Task<int> GetCount(PublicId toId, ReactionType type);
        Task<CollectionObject> Get(PublicId id, int? viewerId);
        Task<bool> HasBlockedViewer(PublicId id, int? viewerId);


        // public long FromPeerId { get; set; }
        // public int FromUserId { get; set; }

        Task<ReactionsSummaryModel> GetSummary(PublicId toId, int? viewerId);
        Task<ReactionsSummaryModel> PutSummary(ReactionsSummaryModel summary);
        Task DeleteSummary(ReactionsSummaryModel summary);



        Task<List<ReactionsSummaryModel>> GetMostLiked(ActivityStreamFilter filter, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostDisliked(ActivityStreamFilter filter, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostFollowed(ActivityStreamFilter filter, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostBlocked(ActivityStreamFilter filter, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostReported(ActivityStreamFilter filter, int? viewerId);
    }
}