using System.Threading.Tasks;
using PinkUmbrella.Models.Search;

namespace PinkUmbrella.Services.Search
{
    public interface ISearchableService
    {
        SearchResultType ResultType { get; }

        SearchSource Source { get; }
        
        string ControllerName { get; }
        
        Task<SearchResultsModel> Search(SearchRequestModel request);
    }
}