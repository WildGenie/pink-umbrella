using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;

namespace PinkUmbrella.Services.NoSql
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<ISearchableService> _searchables;

        public SearchService(IEnumerable<ISearchableService> searchables)
        {
            _searchables = searchables;
        }

        public IEnumerable<ISearchableService> Get(SearchResultType type)
        {
            foreach (var searchable in _searchables) {
                if (searchable.ResultType == type) {
                    yield return searchable;
                }
            }
        }

        public async Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultType? type, SearchResultOrder order, PaginationModel pagination)
        {
            var list = new List<SearchResultModel>();
            int totalResults = 0;
            foreach (var searchable in _searchables) {
                if (type.HasValue && type.Value != searchable.ResultType)
                {
                    continue;
                }
                var results = await searchable.Search(text, viewerId, order, pagination);
                if (results.Results.Count == 0)
                {
                    continue;
                }
                list.AddRange(results.Results);
                totalResults += results.TotalResults;
            }
            return new SearchResultsModel() {
                Results = list,
                TotalResults = totalResults
            };
        }
    }
}