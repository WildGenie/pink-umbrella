using System.Collections.Generic;
using System.Threading.Tasks;
using seattle.Models;

namespace seattle.Services
{
    public interface ISearchService
    {
        Task<SearchResultsModel> Search(string text, SearchResultOrder order, PaginationModel pagination);

        ISearchableService Get(SearchResultType type);
    }
}