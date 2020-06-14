using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface ISearchService
    {
        Task<SearchResultsModel> Search(string text, SearchResultOrder order, PaginationModel pagination);

        ISearchableService Get(SearchResultType type);
    }
}