using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.ViewModels.Account;

namespace PinkUmbrella.Services.Sql
{
    public class UserProfileService : IUserProfileService
    {
        private readonly SignInManager<UserProfileModel> _signInManager;
        private readonly UserManager<UserProfileModel> _userManager;
        private readonly SimpleDbContext _dbContext;

        public UserProfileService(
                UserManager<UserProfileModel> userManager,
                SignInManager<UserProfileModel> signInManager,
                SimpleDbContext dbContext
                ) {
                    _userManager = userManager;
                    _signInManager = signInManager;
                    _dbContext = dbContext;
                }

        public UserProfileModel CreateUser(RegisterInputModel input)
        {
            return new UserProfileModel {
                UserName = input.Email,
                Email = input.Email,
                Handle = input.Handle,
                DisplayName = input.DisplayName,
                WhenCreated = DateTime.UtcNow,
                Bio = "Hey, I'm " + input.DisplayName,
                Visibility = Visibility.VISIBLE_TO_WORLD,
                BioVisibility = Visibility.VISIBLE_TO_WORLD,
                WhenLastLoggedInVisibility = Visibility.VISIBLE_TO_WORLD,
                WhenLastOnlineVisibility = Visibility.VISIBLE_TO_WORLD,
            };
        }

        public async Task DeleteUser(int id, int by_user_id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.WhenDeleted = DateTime.UtcNow;
            user.DeletedByUserId = by_user_id;
            await _userManager.UpdateAsync(user);
        }

        public Task<List<UserProfileModel>> GetMostRecentlyCreatedUsers()
        {
            throw new System.NotImplementedException();
        }

        public async Task<UserProfileModel> GetUser(int id, int? viewerId)
        {   
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                await BindReferences(user, viewerId);
                if (CanView(user, viewerId))
                {
                    return user;
                }
            }
            
            return null;
        }

        private bool CanView(UserProfileModel user, int? viewerId)
        {
            if (viewerId.HasValue && user.Id == viewerId.Value)
            {
                return true;
            }
            else if (user.HasBeenBlockedOrReported)
            {
                return false;
            }

            switch (user.Visibility)
            {
                case Visibility.HIDDEN: return false;
                case Visibility.VISIBLE_TO_FOLLOWERS:
                if (!user.Reactions.Any(r => r.Type == ReactionType.Follow))
                {
                    return false;
                }
                break;
                case Visibility.VISIBLE_TO_REGISTERED:
                if (!viewerId.HasValue)
                {
                    return false;
                }
                break;
            }
            return true;
        }

        public async Task<UserProfileModel> GetUser(string handle, int? viewerId)
        {
            var user =  await _userManager.Users.SingleOrDefaultAsync(u => u.Handle == handle);
            if (user != null)
            {
                await BindReferences(user, viewerId);
                if (CanView(user, viewerId))
                {
                    return user;
                }
            }
            
            return null;
        }

        public async Task LogIn(int id, string from)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.WhenLastLoggedIn = DateTime.UtcNow;
            user.WhenLastOnline = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        public async Task<List<UserProfileModel>> GetFollowers(int userId, int? viewerId)
        {
            var ids = await _dbContext.ProfileReactions.Where(r => r.UserId == userId && r.Type == ReactionType.Follow).Select(r => r.UserId).ToListAsync();
            return await GetUsers(ids, viewerId);
        }

        public async Task<List<UserProfileModel>> GetFollowing(int userId, int? viewerId)
        {
            var ids = await _dbContext.ProfileReactions.Where(r => r.ToId == userId && r.Type == ReactionType.Follow).Select(r => r.ToId).ToListAsync();
            return await GetUsers(ids, viewerId);
        }

        public async Task<List<UserProfileModel>> GetBlocked(int userId, int? viewerId)
        {
            var ids = await _dbContext.ProfileReactions.Where(r => r.UserId == userId && r.Type == ReactionType.Block).Select(r => r.UserId).ToListAsync();
            return await GetUsers(ids, viewerId);
        }

        private async Task<List<UserProfileModel>> GetUsers(List<int> ids, int? viewerId)
        {
            var ret = new List<UserProfileModel>();
            foreach (var id in ids)
            {
                var user = await GetUser(id, viewerId);
                if (user != null)
                {
                    ret.Add(user);
                }
            }
            return ret;
        }

        private async Task BindReferences(UserProfileModel user, int? viewerId)
        {
            if (viewerId.HasValue)
            {
                user.Reactions = await _dbContext.ProfileReactions.Where(r => r.ToId == user.Id && r.UserId == viewerId.Value).ToListAsync();
                var reactionTypes = user.Reactions.Select(r => r.Type).ToHashSet();
                user.HasLiked = reactionTypes.Contains(ReactionType.Like);
                user.HasDisliked = reactionTypes.Contains(ReactionType.Dislike);
                user.HasFollowed = reactionTypes.Contains(ReactionType.Follow);
                user.HasBlocked = reactionTypes.Contains(ReactionType.Block);
                user.HasReported = reactionTypes.Contains(ReactionType.Report);

                var blockOrReport = await _dbContext.ProfileReactions.FirstOrDefaultAsync(r => r.ToId == viewerId.Value && r.UserId == user.Id && (r.Type == ReactionType.Block || r.Type == ReactionType.Report));
                user.HasBeenBlockedOrReported = user.Reactions.Any(r => r.Type == ReactionType.Block || r.Type == ReactionType.Report) || blockOrReport != null;
            }
        }
    }
}