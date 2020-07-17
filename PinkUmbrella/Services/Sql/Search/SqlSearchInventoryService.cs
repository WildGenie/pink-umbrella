using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SqlSearchInventoryService : ISearchableService
    {
        private readonly SimpleDbContext _dbContext;

        public SqlSearchInventoryService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SearchResultType ResultType => SearchResultType.Inventory;

        public SearchSource Source => SearchSource.Sql;

        public string ControllerName => "Inventory";

        public async Task<SearchResultsModel> Search(SearchRequestModel request)
        {
            IQueryable<SimpleInventoryModel> query = _dbContext.Inventories;
            if (!string.IsNullOrWhiteSpace(request.text))
            {
                var textToLower = request.text.ToLower();
                query = query.Where(p => p.DisplayName.ToLower().Contains(textToLower) || p.Description.ToLower().Contains(textToLower));
            }
            
            switch (request.order) {
                // case SearchResultOrder.Top:
                // case SearchResultOrder.Hot:
                // query = query.OrderBy(q => q.LikeCount);
                // break;
                default:
                case SearchResultOrder.Latest:
                query = query.OrderBy(q => q.WhenCreated);
                break;
            }

            var totalCount = query.Count();
            var results = await query.Skip(request.pagination.start).Take(request.pagination.count).ToListAsync();
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