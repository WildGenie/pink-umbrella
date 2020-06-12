using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface IReactionService
    {
        Task React(int userId, int toId, ReactionType type, ReactionSubject subject);
    }
}