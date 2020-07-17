using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;

namespace PinkUmbrella.Services.NoSql
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<ISearchableService> _searchables;

        public SearchService(IEnumerable<ISearchableService> searchables)
        {
            _searchables = searchables.Where(s => s.Source == SearchSource.Elastic);
        }

        public IEnumerable<ISearchableService> Get(SearchResultType type)
        {
            foreach (var searchable in _searchables) {
                if (searchable.ResultType == type) {
                    yield return searchable;
                }
            }
        }

        public async Task<SearchResultsModel> Search(SearchRequestModel request)
        {
            var list = new List<SearchResultModel>();
            int totalResults = 0;
            foreach (var searchable in _searchables) {
                if (request.type.HasValue && request.type.Value != searchable.ResultType)
                {
                    continue;
                }
                var results = await searchable.Search(request);
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