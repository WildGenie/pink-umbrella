using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using System.Collections.Generic;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Models.Search;
using Poncho.Models;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SqlSearchPostsService : ISearchableService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPostService _posts;

        public SqlSearchPostsService(SimpleDbContext dbContext, IPostService posts)
        {
            _dbContext = dbContext;
            _posts = posts;
        }

        public SearchResultType ResultType => SearchResultType.Post;

        public SearchSource Source => SearchSource.Sql;

        public string ControllerName => "Posts";

        public async Task<SearchResultsModel> Search(SearchRequestModel request)
        {
            IQueryable<PostModel> query = _dbContext.Posts;
            if (!string.IsNullOrWhiteSpace(request.text))
            {
                var textToLower = request.text.ToLower();
                query = query.Where(p => p.PostType == PostType.Text && p.Content.ToLower().Contains(textToLower));
            }

            if (request.tags != null && request.tags.Length > 0)
            {
                var tags = await _dbContext.AllTags.Where(t => request.tags.Contains(t.Tag)).Select(t => t.Id).ToArrayAsync();
                query = query.Where(p => _dbContext.PostTags.FirstOrDefault(t => t.ToId == p.Id && tags.Contains(t.TagId)) != null);
            }
            
            switch (request.order) {
                case SearchResultOrder.Top:
                case SearchResultOrder.Hot:
                query = query.OrderBy(q => q.LikeCount).ThenByDescending(q => q.WhenCreated);
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
                await _posts.BindReferences(r, request.viewerId);
                if (_posts.CanView(r, request.viewerId))
                {
                    results.Add(r);
                }
            }
            return new SearchResultsModel() {
                Results = results.Skip(request.pagination.start).Take(request.pagination.count).Select(p => new SearchResultModel() {
                    Type = ResultType,
                    Value = p,
                }).ToList(),
                TotalResults = results.Count()
            };
        }
    }
}