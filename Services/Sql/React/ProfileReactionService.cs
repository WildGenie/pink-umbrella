using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql.React
{
    public class ProfileReactionService : IReactableService
    {
        private readonly UserManager<UserProfileModel> _userManager;
        private readonly SimpleDbContext _dbContext;

        public ProfileReactionService(UserManager<UserProfileModel> userManager, SimpleDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public string ControllerName => "Profile";

        public ReactionSubject Subject => ReactionSubject.Profile;

        public Task<List<int>> GetIds()
        {
            return _userManager.Users.Select(p => p.Id).ToListAsync();
        }

        public async Task RefreshStats(int id)
        {
            var post = await _userManager.FindByIdAsync(id.ToString());
            post.LikeCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id).Count();
            post.DislikeCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id).Count();
            post.ReportCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id).Count();
            await _dbContext.SaveChangesAsync();
        }
    }
}