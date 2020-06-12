using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.ViewModels.Account;

namespace seattle.Services.Sql
{
    public class UserProfileService : IUserProfileService
    {
        protected readonly SignInManager<UserProfileModel> _signInManager;
        protected readonly UserManager<UserProfileModel> _userManager;

        public UserProfileService(
                UserManager<UserProfileModel> userManager,
                SignInManager<UserProfileModel> signInManager
                ) {
                    _userManager = userManager;
                    _signInManager = signInManager;
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

        public Task<UserProfileModel> GetUser(int id)
        {
            return _userManager.FindByIdAsync(id.ToString());
        }

        public async Task<UserProfileModel> GetUser(string handle)
        {
            return await _userManager.Users.SingleOrDefaultAsync(u => u.Handle == handle);
        }

        public async Task LogIn(int id, string from)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            user.WhenLastLoggedIn = DateTime.UtcNow;
            user.WhenLastOnline = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }
}