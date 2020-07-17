using System.Threading.Tasks;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Public;
using Nest;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchProfilesService : BaseSearchElasticService<PublicProfileModel>, ISearchableService
    {
        private readonly IPublicProfileService _profiles;

        public ElasticSearchProfilesService(IPublicProfileService profiles)
        {
            _profiles = profiles;
        }

        public SearchResultType ResultType => SearchResultType.Profile;

        public SearchSource Source => SearchSource.Elastic;

        public string ControllerName => "Profile";

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
                        new MatchQuery() { Field = "displayName", Query = request.text, Boost = .8 },
                        new MatchQuery() { Field = "handle", Query = request.text, Boost = 1 },
                        new MatchQuery() { Field = "bio", Query = request.text, Boost = 0.5 },
                    }
                });
            }

            AddTagSearch(request, musts);
            return await DoSearch(request, elastic, new BoolQuery()
            {
                Must = musts,
            }, null, ResultType);
        }

        protected override async Task<bool> CanView(PublicProfileModel r, int? viewerId)
        {
            await _profiles.BindReferences(r, viewerId);
            return _profiles.CanView(r, viewerId);
        }
    }
}