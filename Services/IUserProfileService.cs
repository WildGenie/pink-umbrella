using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.ViewModels.Account;

namespace PinkUmbrella.Services
{
    public interface IUserProfileService
    {
        Task<PaginatedModel<UserProfileModel>> GetMostRecentlyCreatedUsers();

        Task<UserProfileModel> GetUser(int id, int? viewerId);

        Task<UserProfileModel> GetUser(string handle, int? viewerId);

        UserProfileModel CreateUser(RegisterInputModel initial);

        Task DeleteUser(int id, int by_user_id);

        Task LogIn(int id, string from);

        Task<List<UserProfileModel>> GetFollowers(int userId, int? viewerId);

        Task<List<UserProfileModel>> GetFollowing(int userId, int? viewerId);

        Task<List<UserProfileModel>> GetBlocked(int userId, int? viewerId);

        Task BindReferences(UserProfileModel user, int? viewerId);

        bool CanView(UserProfileModel user, int? viewerId);

        Task<GroupAccessCodeModel> NewGroupAccessCode(int createdByUserId, int forUserId, string group);

        Task<GroupAccessCodeModel> GetGroupAccessCodeAsync(string code, int userId);
        
        Task ConsumeGroupAccessCodeAsync(UserProfileModel user, GroupAccessCodeModel code);

        Task MakeFirstUserDev(UserProfileModel user);

        Task<List<GroupAccessCodeModel>> GetUnusedUnexpiredAccessCodes();
    }
}