using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface IReactionService
    {
        Task<int> React(int userId, PublicId toId, ReactionType type, ReactionSubject subject);
        Task UnReact(int userId, PublicId toId, ReactionType type, ReactionSubject subject);
        Task<int> GetCount(PublicId toId, ReactionType type, ReactionSubject subject);
        Task<List<ReactionModel>> Get(ReactionSubject subject, PublicId id, int? viewerId);
        Task<bool> HasBlockedViewer(ReactionSubject subject, PublicId id, int? viewerId);
    }
}