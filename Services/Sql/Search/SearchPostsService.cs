using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using System.Collections.Generic;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SearchPostsService : ISearchableService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPostService _posts;

        public SearchPostsService(SimpleDbContext dbContext, IPostService posts)
        {
            _dbContext = dbContext;
            _posts = posts;
        }

        public SearchResultType ResultType => SearchResultType.Post;

        public string ControllerName => "Posts";

        public async Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination)
        {
            IQueryable<PostModel> query = _dbContext.Posts;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var textToLower = text.ToLower();
                query = query.Where(p => p.PostType == PostType.Text && p.Content.ToLower().Contains(textToLower));
            }
            
            switch (order) {
                case SearchResultOrder.Top:
                case SearchResultOrder.Hot:
                query = query.OrderBy(q => q.LikeCount);
                break;
                default:
                case SearchResultOrder.Latest:
                query = query.OrderBy(q => q.WhenCreated);
                break;
            }
            
            var searchResults = await query.ToListAsync();
            var results = new List<PostModel>();
            foreach (var r in searchResults)
            {
                await _posts.BindReferences(r, viewerId);
                if (_posts.CanView(r, viewerId))
                {
                    results.Add(r);
                }
            }
            return new SearchResultsModel() {
                Results = results.Skip(pagination.start).Take(pagination.count).Select(p => new SearchResultModel() {
                    Type = ResultType,
                    Value = p,
                }).ToList(),
                TotalResults = results.Count()
            };
        }
    }
}