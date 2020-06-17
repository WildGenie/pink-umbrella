using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface ISearchService
    {
        Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultType? type, SearchResultOrder order, PaginationModel pagination);

        IEnumerable<ISearchableService> Get(SearchResultType type);
    }
}