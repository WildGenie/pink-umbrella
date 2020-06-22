using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.ViewModels.Account;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PinkUmbrella.Services.Sql
{
    public class UserProfileService : IUserProfileService
    {
        private readonly SignInManager<UserProfileModel> _signInManager;
        private readonly UserManager<UserProfileModel> _userManager;
        private readonly RoleManager<UserGroupModel> _roleManager;
        private readonly SimpleDbContext _dbContext;
        private readonly StringRepository _strings;

        public UserProfileService(
                UserManager<UserProfileModel> userManager,
                SignInManager<UserProfileModel> signInManager,
                RoleManager<UserGroupModel> roleManager,
                SimpleDbContext dbContext,
                StringRepository strings
                ) {
                    _userManager = userManager;
                    _signInManager = signInManager;
                    _roleManager = roleManager;
                    _dbContext = dbContext;
                    _strings = strings;
                }

        public async Task<UserProfileModel> CreateUser(RegisterInputModel input, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(input.Email))
            {
                modelState.AddModelError(nameof(input.Email), "Email is empty");
            }
            else if (!_strings.ValidEmail(input.Email))
            {
                modelState.AddModelError(nameof(input.Email), "Email is invalid");
            }
            else if (string.IsNullOrWhiteSpace(input.Handle))
            {
                modelState.AddModelError(nameof(input.Handle), "Handle is empty");
            }
            else if (!_strings.ValidHandleRegex.IsMatch(input.Handle))
            {
                modelState.AddModelError(nameof(input.Handle), "Handle is invalid");
            }
            else if (await HandleExists(input.Handle))
            {
                modelState.AddModelError(nameof(input.Handle), "Handle already used");
            }
            else
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
            return null;
        }

        public async Task DeleteUser(int id, int by_user_id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.WhenDeleted = DateTime.UtcNow;
            user.DeletedByUserId = by_user_id;
            await _userManager.UpdateAsync(user);
        }

        public async Task<PaginatedModel<UserProfileModel>> GetMostRecentlyCreatedUsers()
        {
            return new PaginatedModel<UserProfileModel>() {
                Items = await _userManager.Users.OrderByDescending(u => u.Id).Take(10).ToListAsync(),
                Total = _userManager.Users.Count(),
                Pagination = new PaginationModel(),
            };
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

        public bool CanView(UserProfileModel user, int? viewerId)
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

        public async Task<List<UserProfileModel>> GetFollowing(int userId, int? viewerId)
        {
            var ids = await _dbContext.ProfileReactions.Where(r => r.UserId == userId && r.Type == ReactionType.Follow).Select(r => r.ToId).ToListAsync();
            return await GetUsers(ids, viewerId);
        }

        public async Task<List<UserProfileModel>> GetFollowers(int userId, int? viewerId)
        {
            var ids = await _dbContext.ProfileReactions.Where(r => r.ToId == userId && r.Type == ReactionType.Follow).Select(r => r.UserId).ToListAsync();
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

        public async Task BindReferences(UserProfileModel user, int? viewerId)
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
                user.HasBeenBlockedOrReported = blockOrReport != null;
            }
        }

        public async Task<GroupAccessCodeModel> GetGroupAccessCodeAsync(string code, int userId)
        {
            var c = await _dbContext.GroupAccessCodes.SingleOrDefaultAsync(c => c.WhenConsumed == null && c.Code == code && c.ForUserId == userId);
            if (c != null && c.WhenExpires >= DateTime.UtcNow)
            {
                return c;
            }
            else
            {
                return null;
            }
        }

        public async Task ConsumeGroupAccessCodeAsync(UserProfileModel user, GroupAccessCodeModel c)
        {
            if (c.WhenExpires >= DateTime.UtcNow)
            {
                await _userManager.AddToRoleAsync(user, c.GroupName);
                c.WhenConsumed = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<GroupAccessCodeModel> NewGroupAccessCode(GroupAccessCodeModel info)
        {
            info.Code = Guid.NewGuid().ToString();
            info.WhenCreated = DateTime.UtcNow;
            info.WhenExpires = DateTime.UtcNow.AddDays(1);
            _dbContext.GroupAccessCodes.Add(info);
            await _dbContext.SaveChangesAsync();
            return info;
        }

        public Task<GroupAccessCodeModel> NewGroupAccessCode(int createdByUserId, int forUserId, string group)
        {
            return NewGroupAccessCode(new GroupAccessCodeModel() {
                CreatedByUserId = createdByUserId,
                ForUserId = forUserId,
                GroupName = group,
            });
        }

        public async Task MakeFirstUserDev(UserProfileModel user)
        {
            if (_userManager.Users.Count() == 1)
            {
                if (!await _roleManager.RoleExistsAsync("dev"))
                {
                    var result = await _roleManager.CreateAsync(new UserGroupModel() { Name = "dev", OwnerId = -1, GroupType = GroupType.Dev });
                    if (!result.Succeeded)
                    {
                        throw new Exception("Cannot create dev group");
                    }
                }
                if (!await _userManager.IsInRoleAsync(user, "dev"))
                {
                    await _userManager.AddToRoleAsync(user, "dev");
                }
            }
        }

        public async Task<List<GroupAccessCodeModel>> GetUnusedUnexpiredAccessCodes()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.GroupAccessCodes.Where(c => c.WhenConsumed == null && c.WhenExpires < now).ToListAsync();
        }

        public async Task<bool> HandleExists(string handle)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Handle.ToLower() == handle);
            return user != null;
        }
    }
}