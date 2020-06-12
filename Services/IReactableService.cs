using System.Collections.Generic;
using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface IReactableService
    {
        string ControllerName { get; }
        ReactionSubject Subject { get; }

        Task RefreshStats(int id);
        Task<List<int>> GetIds();
    }
}