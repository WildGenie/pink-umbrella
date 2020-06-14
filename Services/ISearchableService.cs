using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface ISearchableService
    {
        SearchResultType ResultType { get; }
        string ControllerName { get; }
        Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination);
    }
}