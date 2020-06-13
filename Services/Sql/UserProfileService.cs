using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;
using seattle.ViewModels.Account;

namespace seattle.Services.Sql
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
                Bio = "",
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
            await BindReferences(user, viewerId);
            return user;
        }

        public async Task<UserProfileModel> GetUser(string handle, int? viewerId)
        {
            var user =  await _userManager.Users.SingleOrDefaultAsync(u => u.Handle == handle);
            await BindReferences(user, viewerId);
            return user;
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
                ret.Add(await GetUser(id, viewerId));
            }
            return ret;
        }

        private async Task BindReferences(UserProfileModel user, int? viewerId)
        {
            var reactions = await _dbContext.PostReactions.Where(r => r.UserId == viewerId && r.ToId == user.Id).Select(r => r.Type).ToListAsync();

            user.HasLiked = reactions.Contains(ReactionType.Like);
            user.HasDisliked = reactions.Contains(ReactionType.Dislike);
            user.HasFollowed = reactions.Contains(ReactionType.Follow);
            user.HasBlocked = reactions.Contains(ReactionType.Block);
            user.HasReported = reactions.Contains(ReactionType.Report);
        }
    }
}