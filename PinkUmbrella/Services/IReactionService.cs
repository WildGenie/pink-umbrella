using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IReactionService
    {
        Task<int> React(int userId, PublicId toId, ReactionType type, ReactionSubject subject);
        Task UnReact(int userId, PublicId toId, ReactionType type, ReactionSubject subject);
        Task<int> GetCount(PublicId toId, ReactionType type, ReactionSubject subject);
        Task<CollectionObject> Get(ReactionSubject subject, PublicId id, int? viewerId);
        Task<bool> HasBlockedViewer(ReactionSubject subject, PublicId id, int? viewerId);


        // public long FromPeerId { get; set; }
        // public int FromUserId { get; set; }

        Task<ReactionsSummaryModel> GetSummary(int toId, ReactionSubject subject, int? viewerId);
        Task<ReactionsSummaryModel> PutSummary(ReactionsSummaryModel summary, ReactionSubject subject);
        Task DeleteSummary(ReactionsSummaryModel summary, ReactionSubject subject);



        Task<List<ReactionsSummaryModel>> GetMostLiked(ReactionSubject subject, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostDisliked(ReactionSubject subject, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostFollowed(ReactionSubject subject, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostBlocked(ReactionSubject subject, int? viewerId);
        Task<List<ReactionsSummaryModel>> GetMostReported(ReactionSubject subject, int? viewerId);
    }
}