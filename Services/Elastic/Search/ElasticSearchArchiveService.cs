using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;
using System.Collections.Generic;
using Nest;

namespace PinkUmbrella.Services.Elastic.Search
{
    public abstract class ElasticSearchArchiveService : BaseSearchElasticService<ArchivedMediaModel>, ISearchableService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IArchiveService _archive;

        public ElasticSearchArchiveService(SimpleDbContext dbContext, IArchiveService archive)
        {
            _dbContext = dbContext;
            _archive = archive;
        }

        public string ControllerName => "Archive";

        public abstract SearchResultType ResultType { get; }

        public SearchSource Source => SearchSource.Elastic;

        public async Task<SearchResultsModel> Search(SearchRequestModel request, ArchivedMediaType mediaType)
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

        protected override async Task<bool> CanView(ArchivedMediaModel r, int? viewerId)
        {
            await _archive.BindReferences(r, viewerId);
            return _archive.CanView(r, viewerId);
        }

        public abstract Task<SearchResultsModel> Search(SearchRequestModel request);
    }
}