using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using System.Collections.Generic;
using PinkUmbrella.Models.Search;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Services.Local;
using PinkUmbrella.Repositories;
using Tides.Actors;

namespace PinkUmbrella.Services.Sql.Search
{
    public class SqlSearchProfilesService : ISearchableService
    {
        private readonly UserManager<UserProfileModel> _userManager;
        private readonly IUserProfileService _profiles;
        private readonly IPublicProfileService _publicProfiles;
        private readonly SimpleDbContext _dbContext;

        public SqlSearchProfilesService(UserManager<UserProfileModel> userManager, 
            IUserProfileService profiles,
            IPublicProfileService publicProfiles,
            SimpleDbContext dbContext)
        {
            _userManager = userManager;
            _profiles = profiles;
            _publicProfiles = publicProfiles;
            _dbContext = dbContext;
        }

        public SearchResultType ResultType => SearchResultType.Profile;

        public SearchSource Source => SearchSource.Sql;

        public string ControllerName => "Profile";

        public async Task<SearchResultsModel> Search(SearchRequestModel request)
        {
            var query = _userManager.Users;
            if (!string.IsNullOrWhiteSpace(request.text))
            {
                var textToLower = request.text.ToLower();
                query = query.Where(p => p.DisplayName.ToLower().Contains(textToLower) || p.Handle.ToLower().Contains(textToLower));
            }

            if (request.tags != null && request.tags.Length > 0)
            {
                var tags = await _dbContext.AllTags.Where(t => request.tags.Contains(t.Tag)).Select(t => t.Id).ToArrayAsync();
                query = query.Where(p => _dbContext.ProfileTags.FirstOrDefault(t => t.ToId == p.Id && tags.Contains(t.TagId)) != null);
            }
            
            switch (request.order) {
                case SearchResultOrder.Top:
                case SearchResultOrder.Hot:
                query = query.OrderByDescending(q => q.FollowCount).ThenByDescending(q => q.LikeCount).ThenByDescending(q => q.WhenCreated);
                break;
                default:
                case SearchResultOrder.Latest:
                query = query.OrderByDescending(q => q.WhenCreated);
                break;
            }

            var searchResults = await query.ToListAsync();
            var results = new List<ActorObject>();
            foreach (var r in searchResults)
            {
                var asPublic = await _publicProfiles.Transform(r, 0, request.viewerId);
                if (asPublic != null)
                {
                    results.Add(asPublic);
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