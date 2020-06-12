using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql.Search
{
    public class SearchShopsService : ISearchableService
    {
        private readonly SimpleDbContext _dbContext;

        public SearchShopsService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SearchResultType ResultType => SearchResultType.Shop;

        public string ControllerName => "Shop";

        public async Task<SearchResultsModel> Search(string text, SearchResultOrder order, PaginationModel pagination)
        {
            var query = _dbContext.Shops.Where(p => p.Handle.Contains(text) || p.DisplayName.Contains(text));

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

            var totalCount = query.Count();
            var results = await query.Skip(pagination.start).Take(pagination.count).ToListAsync();
            return new SearchResultsModel() {
                Results = results.Select(p => new SearchResultModel() {
                    Type = ResultType,
                    Value = p,
                }).ToList(),
                TotalResults = totalCount
            };
        }
    }
}