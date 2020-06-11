using System.Collections.Generic;
using seattle.Models;

namespace seattle.Services
{
    public interface IUserProfileService
    {
        List<UserProfileModel> GetMostRecentlyCreatedUsers();

        List<UserProfileModel> SearchUsers(string text, PaginationModel pagination);

        UserProfileModel GetUser(int id);

        UserProfileModel GetUser(string handle);

        UserProfileModel CreateUser(UserProfileModel initial);

        void DeleteUser(int id, int by_user_id);
    }
}