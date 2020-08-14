using System.Threading.Tasks;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;
using System.Collections.Generic;
using Nest;
using Estuary.Actors;
using Estuary.Services;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchShopsService : BaseSearchElasticService<Common.Organization>, ISearchableService
    {
        private readonly IShopService _shops;

        public ElasticSearchShopsService(IShopService shops, IHazActivityStreamPipe pipe): base(pipe)
        {
            _shops = shops;
        }

        public SearchResultType ResultType => SearchResultType.Shop;

        public SearchSource Source => SearchSource.Elastic;

        public string ControllerName => "Shop";

        public async Task<SearchResultsModel> Search(SearchRequestModel request)
        {
            var elastic = GetClient();
            var musts = new List<QueryContainer>();

            if (!string.IsNullOrWhiteSpace(request.text))
            {
                musts.Add(new BoolQuery
                {
                    Should = new List<QueryContainer>
                    {
                        new MatchQuery() { Field = "displayName", Query = request.text },
                        new MatchQuery() { Field = "description", Query = request.text },
                        new MatchQuery() { Field = "streetAddress", Query = request.text },
                        new MatchQuery() { Field = "zipCode", Query = request.text },
                    }
                });
            }

            AddTagSearch(request, musts);
            return await DoSearch(request, elastic, new BoolQuery()
            {
                Must = musts,
            }, null, ResultType);
        }
    }
}