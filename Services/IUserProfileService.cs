using System.Collections.Generic;
using seattle.Models;
using seattle.ViewModels.Account;

namespace seattle.Services
{
    public interface IUserProfileService
    {
        List<UserProfileModel> GetMostRecentlyCreatedUsers();

        List<UserProfileModel> SearchUsers(string text, PaginationModel pagination);

        UserProfileModel GetUser(int id);

        UserProfileModel GetUser(string handle);

        UserProfileModel CreateUser(RegisterInputModel initial);

        void DeleteUser(int id, int by_user_id);

        void LogIn(int id, string from);
    }
}