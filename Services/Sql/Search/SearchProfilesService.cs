using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using System.Collections.Generic;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SearchProfilesService : ISearchableService
    {
        private readonly UserManager<UserProfileModel> _userManager;
        private readonly IUserProfileService _profiles;

        public SearchProfilesService(UserManager<UserProfileModel> userManager, IUserProfileService profiles)
        {
            _userManager = userManager;
            _profiles = profiles;
        }

        public SearchResultType ResultType => SearchResultType.Profile;

        public string ControllerName => "Profile";

        public async Task<SearchResultsModel> Search(string text, int? viewerId, SearchResultOrder order, PaginationModel pagination)
        {
            var query = _userManager.Users;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var textToLower = text.ToLower();
                query = query.Where(p => p.DisplayName.ToLower().Contains(textToLower) || p.Handle.ToLower().Contains(textToLower));
            }
            
            switch (order) {
                case SearchResultOrder.Top:
                case SearchResultOrder.Hot:
                query = query.OrderByDescending(q => q.FollowCount).ThenByDescending(q => q.LikeCount);
                break;
                default:
                case SearchResultOrder.Latest:
                query = query.OrderByDescending(q => q.WhenCreated);
                break;
            }
            //query = query.ThenByDescending(q => q.WhenCreated);

            var searchResults = await query.ToListAsync();
            var results = new List<UserProfileModel>();
            foreach (var r in searchResults)
            {
                await _profiles.BindReferences(r, viewerId);
                if (_profiles.CanView(r, viewerId))
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