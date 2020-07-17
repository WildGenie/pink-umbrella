using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;

namespace PinkUmbrella.Services
{
    public interface ISearchService
    {
        Task<SearchResultsModel> Search(SearchRequestModel request);

        IEnumerable<ISearchableService> Get(SearchResultType type);
    }
}