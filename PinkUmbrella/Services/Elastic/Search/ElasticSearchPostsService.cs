using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;
using Nest;
using System.Collections.Generic;
using Tides.Models;

namespace PinkUmbrella.Services.Elastic.Search
{
    public class ElasticSearchPostsService : BaseSearchElasticService<PostModel>, ISearchableService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPostService _posts;

        public ElasticSearchPostsService(SimpleDbContext dbContext, IPostService posts)
        {
            _dbContext = dbContext;
            _posts = posts;
        }

        public SearchResultType ResultType => SearchResultType.Post;

        public SearchSource Source => SearchSource.Elastic;

        public string ControllerName => "Posts";

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
                        new MatchQuery() { Field = "content", Query = request.text },
                    }
                });
            }

            AddTagSearch(request, musts);
            return await DoSearch(request, elastic, new BoolQuery()
            {
                Must = musts,
            }, new TermQuery { Name = "postType", Value = PostType.Text }, ResultType);
        }

        protected override async Task<bool> CanView(PostModel r, int? viewerId)
        {
            await _posts.BindReferences(r, viewerId);
            return _posts.CanView(r, viewerId);
        }
    }
}