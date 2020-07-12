using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;

namespace PinkUmbrella.Services.Elastic
{
    public abstract class BaseSearchElasticService<T>: BaseElasticService where T: class
    {
        protected async Task<SearchResultsModel> HandleResponse(SearchRequestModel request, Nest.ISearchResponse<T> response, SearchResultType resultType)
        {
            if (response.IsValid && response.Documents.Count > 0)
            {
                var sources = response.HitsMetadata.Hits.Select(h => h.Source);
                var highlights = response.HitsMetadata.Hits.Select(h => h.Highlight);
                var results = new List<T>();
                foreach (var r in sources)
                {
                    if (await CanView(r, request.viewerId))
                    {
                        results.Add(r);
                    }
                }
                return new SearchResultsModel()
                {
                    Results = results.Skip(request.pagination.start).Take(request.pagination.count).Select(p => new SearchResultModel()
                    {
                        Type = resultType,
                        Value = p,
                    }).ToList(),
                    TotalResults = results.Count()
                };
            }
            return new SearchResultsModel();
        }

        protected void AddTagSearch(SearchRequestModel request, List<QueryContainer> musts)
        {
            if (request.tags != null && request.tags.Length > 0)
            {
                musts.Add(new BoolQuery
                {
                    Should = request.tags.Select(t => (QueryContainer) new MatchQuery() { Field = "tags", Query = t }).ToList()
                });
            }
        }

        protected async Task<SearchResultsModel> DoSearch(SearchRequestModel request, ElasticClient elastic, QueryContainer query, QueryContainer filter, SearchResultType resultType)
        {
            var searchRequest = new SearchRequest<T>
            {
                Query = query,
                PostFilter = filter,
                From = request.pagination.start,
                Size = request.pagination.count,
            };
            var response = await elastic.SearchAsync<T>(searchRequest);
            return await HandleResponse(request, response, resultType);
        }

        protected abstract Task<bool> CanView(T r, int? viewerId);
    }
}