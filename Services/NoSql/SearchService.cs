using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Services;

namespace PinkUmbrella.Services.NoSql
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<ISearchableService> _searchables;

        public SearchService(IEnumerable<ISearchableService> searchables)
        {
            _searchables = searchables;
        }

        public ISearchableService Get(SearchResultType type)
        {
            foreach (var searchable in _searchables) {
                if (searchable.ResultType == type) {
                    return searchable;
                }
            }
            return null;
        }

        public async Task<SearchResultsModel> Search(string text, SearchResultOrder order, PaginationModel pagination)
        {
            var list = new List<SearchResultModel>();
            int totalResults = 0;
            foreach (var searchable in _searchables) {
                var results = await searchable.Search(text, order, pagination);
                list.AddRange(results.Results);
                totalResults += results.TotalResults;
            }

            // list.Sort(r => r.WhenCreated);

            return new SearchResultsModel() {
                Results = list,
                TotalResults = totalResults
            };
        }
    }
}