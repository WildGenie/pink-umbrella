using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SearchProfilesService : ISearchableService
    {
        private readonly UserManager<UserProfileModel> _userManager;

        public SearchProfilesService(UserManager<UserProfileModel> userManager)
        {
            _userManager = userManager;
        }

        public SearchResultType ResultType => SearchResultType.Profile;

        public string ControllerName => "Profile";

        public async Task<SearchResultsModel> Search(string text, SearchResultOrder order, PaginationModel pagination)
        {
            var query = _userManager.Users.Where(p => p.DisplayName.Contains(text) || p.Handle.Contains(text));
            
            switch (order) {
                case SearchResultOrder.Top:
                case SearchResultOrder.Hot:
                query = query.OrderBy(q => q.LikeCount);
                break;
                default:
                case SearchResultOrder.Latest:
                query = query.OrderBy(q => q.WhenLastOnline);
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