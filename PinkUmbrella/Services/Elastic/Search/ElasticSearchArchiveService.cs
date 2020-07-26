using System.Threading.Tasks;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;
using System.Collections.Generic;
using Nest;
using Tides.Objects;
using Tides.Core;
using Tides.Services;

namespace PinkUmbrella.Services.Elastic.Search
{
    public abstract class ElasticSearchArchiveService : BaseSearchElasticService<BaseObject>, ISearchableService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IArchiveService _archive;

        public ElasticSearchArchiveService(SimpleDbContext dbContext, IArchiveService archive, IHazActivityStreamPipe pipe): base(pipe)
        {
            _dbContext = dbContext;
            _archive = archive;
        }

        public string ControllerName => "Archive";

        public abstract SearchResultType ResultType { get; }

        public SearchSource Source => SearchSource.Elastic;

        public async Task<SearchResultsModel> Search(SearchRequestModel request, CommonMediaType mediaType)
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

        public abstract Task<SearchResultsModel> Search(SearchRequestModel request);
    }
}