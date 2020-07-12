using System.Threading.Tasks;
using System.Linq;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;
using Nest;
using System.Collections.Generic;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchInventoryService : BaseSearchElasticService<SimpleInventoryModel>, ISearchableService
    {
        private readonly ISimpleInventoryService _inventories;

        public ElasticSearchInventoryService(ISimpleInventoryService inventories)
        {
            _inventories = inventories;
        }

        public SearchResultType ResultType => SearchResultType.Inventory;

        public SearchSource Source => SearchSource.Elastic;

        public string ControllerName => "Inventory";

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
                        new MatchQuery() { Field = "displayName", Query = request.text, Boost = 1.5 },
                        new MatchQuery() { Field = "description", Query = request.text },
                    }
                });
            }

            AddTagSearch(request, musts);
            return await DoSearch(request, elastic, new BoolQuery()
            {
                Must = musts,
            }, null, ResultType);
        }

        protected override async Task<bool> CanView(SimpleInventoryModel r, int? viewerId)
        {
            //await _inventories.BindReferences(r, viewerId);
            return true;//_inventories.CanView(r, viewerId);
        }
    }
}