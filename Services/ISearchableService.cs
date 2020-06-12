using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface ISearchableService
    {
        SearchResultType ResultType { get; }
        string ControllerName { get; }
        Task<SearchResultsModel> Search(string text, SearchResultOrder order, PaginationModel pagination);
    }
}