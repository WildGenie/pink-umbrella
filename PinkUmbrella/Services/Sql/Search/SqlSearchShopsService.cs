// using System.Threading.Tasks;
// using System.Linq;
// using Microsoft.EntityFrameworkCore;
// using PinkUmbrella.Models;
// using PinkUmbrella.Repositories;
// using PinkUmbrella.Services.Search;
// using PinkUmbrella.Models.Search;

// namespace PinkUmbrella.Services.Sql.Search
// {
//     public class SqlSearchShopsService : ISearchableService
//     {
//         private readonly SimpleDbContext _dbContext;

//         public SqlSearchShopsService(SimpleDbContext dbContext)
//         {
//             _dbContext = dbContext;
//         }

//         public SearchResultType ResultType => SearchResultType.Shop;

//         public SearchSource Source => SearchSource.Sql;

//         public string ControllerName => "Shop";

//         public async Task<SearchResultsModel> Search(SearchRequestModel request)
//         {
//             var query = _dbContext.Shops.Where(s => s.WhenDeleted == null);
//             if (!string.IsNullOrWhiteSpace(request.text))
//             {
//                 var textToLower = request.text.ToLower();
//                 query = query.Where(p => p.Handle.ToLower().Contains(textToLower) || p.DisplayName.ToLower().Contains(textToLower) || p.Description.ToLower().Contains(textToLower));
//             }

//             if (request.tags != null && request.tags.Length > 0)
//             {
//                 var tags = await _dbContext.AllTags.Where(t => request.tags.Contains(t.Tag)).Select(t => t.Id).ToArrayAsync();
//                 query = query.Where(p => _dbContext.ShopTags.FirstOrDefault(t => t.ToId == p.Id && tags.Contains(t.TagId)) != null);
//             }

//             switch (request.order) {
//                 case SearchResultOrder.Top:
//                 case SearchResultOrder.Hot:
//                 query = query.OrderBy(q => q.LikeCount);
//                 break;
//                 default:
//                 case SearchResultOrder.Latest:
//                 query = query.OrderBy(q => q.WhenCreated);
//                 break;
//             }

//             var totalCount = query.Count();
//             var results = await query.Skip(request.pagination.start).Take(request.pagination.count).ToListAsync();
//             return new SearchResultsModel() {
//                 Results = results.Select(p => new SearchResultModel() {
//                     Type = ResultType,
//                     Value = p,
//                 }).ToList(),
//                 TotalResults = totalCount
//             };
//         }
//     }
// }