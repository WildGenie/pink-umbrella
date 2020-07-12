using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Models.Public;

namespace PinkUmbrella.Services.Sql.React
{
    // //).ProfileReactions.Where(r => r.ToId == user.UserId && r.ToPeerId == user.PeerId && r.UserId == viewerId.Value && r.FromPeerId == 0).ToListAsync()
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

        public async Task RefreshStats(PublicId id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.LikeCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Like && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            user.DislikeCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Dislike && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            user.ReportCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Report && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            user.BlockCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Block && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            user.FollowCount = _dbContext.ProfileReactions.Where(r => r.Type == ReactionType.Follow && r.ToId == id.Id && r.ToPeerId == id.PeerId).Count();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCount(PublicId id, ReactionType type)
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