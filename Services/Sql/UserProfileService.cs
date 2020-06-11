using System.Collections.Generic;
using seattle.Models;

namespace seattle.Services.Sql
{
    public class UserProfileService : IUserProfileService
    {
        public UserProfileModel CreateUser(UserProfileModel initial)
        {
            throw new System.NotImplementedException();
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

        public List<UserProfileModel> SearchUsers(string text, PaginationModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}