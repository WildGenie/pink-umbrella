using System.Collections.Generic;
using System.Threading.Tasks;
using seattle.Models;
using seattle.ViewModels.Account;

namespace seattle.Services
{
    public interface IUserProfileService
    {
        List<UserProfileModel> GetMostRecentlyCreatedUsers();

        List<UserProfileModel> SearchUsers(string text, PaginationModel pagination);

        Task<UserProfileModel> GetUser(int id);

        Task<UserProfileModel> GetUser(string handle);

        UserProfileModel CreateUser(RegisterInputModel initial);

        Task DeleteUser(int id, int by_user_id);

        Task LogIn(int id, string from);
    }
}