using System;
using System.Collections.Generic;
using seattle.Models;
using seattle.ViewModels.Account;

namespace seattle.Services.Sql
{
    public class UserProfileService : IUserProfileService
    {
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

        public void DeleteUser(int id, int by_user_id)
        {
            throw new System.NotImplementedException();
        }

        public List<UserProfileModel> GetMostRecentlyCreatedUsers()
        {
            throw new System.NotImplementedException();
        }

        public UserProfileModel GetUser(int id)
        {
            throw new System.NotImplementedException();
        }

        public UserProfileModel GetUser(string handle)
        {
            throw new System.NotImplementedException();
        }

        public void LogIn(int id, string from)
        {
            throw new System.NotImplementedException();
        }

        public List<UserProfileModel> SearchUsers(string text, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}