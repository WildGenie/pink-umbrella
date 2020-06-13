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
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.LikeCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id).Count();
            user.DislikeCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id).Count();
            user.ReportCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id).Count();
            user.BlockCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Block && r.ToId == id).Count();
            user.FollowCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Follow && r.ToId == id).Count();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCount(int id, ReactionType type)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            switch (type)
            {
                case ReactionType.Like: return user.LikeCount;
                case ReactionType.Dislike: return user.DislikeCount;
                case ReactionType.Report: return user.ReportCount;
                case ReactionType.Block: return user.BlockCount;
                case ReactionType.Follow: return user.FollowCount;
                default: return 0;
            }
        }
    }
}