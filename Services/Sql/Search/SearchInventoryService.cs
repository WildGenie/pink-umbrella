using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SearchInventoryService : ISearchableService
    {
        private readonly SimpleDbContext _dbContext;

        public SearchInventoryService(SimpleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public SearchResultType ResultType => SearchResultType.Inventory;

        public string ControllerName => "Inventory";

        public async Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination)
        {
            IQueryable<SimpleInventoryModel> query = _dbContext.Inventories;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var textToLower = text.ToLower();
                query = query.Where(p => p.DisplayName.ToLower().Contains(textToLower) || p.Description.ToLower().Contains(textToLower));
            }
            
            switch (order) {
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