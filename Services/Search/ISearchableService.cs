using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;

namespace PinkUmbrella.Services.Search
{
    public interface ISearchableService
    {
        SearchResultType ResultType { get; }
        string ControllerName { get; }
        Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination);
    }
}