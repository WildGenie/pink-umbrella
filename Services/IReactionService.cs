using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface IReactionService
    {
        Task<int> React(int userId, int toId, ReactionType type, ReactionSubject subject);
        Task UnReact(int userId, int toId, ReactionType type, ReactionSubject subject);
    }
}