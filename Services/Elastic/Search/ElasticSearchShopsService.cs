using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;
using System.Collections.Generic;
using Nest;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchShopsService : BaseSearchElasticService<ShopModel>, ISearchableService
    {
        private readonly IShopService _shops;

        public ElasticSearchShopsService(IShopService shops)
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

        protected override async Task<bool> CanView(ShopModel r, int? viewerId)
        {
            await _shops.BindReferences(r, viewerId);
            return _shops.CanView(r, viewerId);
        }
    }
}