using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface IReactableService
    {
        string ControllerName { get; }
        ReactionSubject Subject { get; }

        Task<int> GetCount(int toId, ReactionType type);

        Task RefreshStats(int id);
        Task<List<int>> GetIds();
    }
}