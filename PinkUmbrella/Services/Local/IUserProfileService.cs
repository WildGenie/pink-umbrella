using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Public;
using PinkUmbrella.ViewModels.Account;
using Tides.Models;
using Tides.Models.Auth;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Local
{
    public interface IUserProfileService
    {
        Task<PaginatedModel<UserProfileModel>> GetMostRecentlyCreatedUsers();

        Task<UserProfileModel> GetUser(int id, int? viewerId);

        Task<UserProfileModel> GetUser(string handle, int? viewerId);

        Task<UserProfileModel> CreateUser(RegisterInputModel initial, ModelStateDictionary modelState);

        Task DeleteUser(int id, int by_user_id);

        Task LogIn(int id, string from);

        Task<List<UserProfileModel>> GetFollowers(int userId, int? viewerId);

        Task<List<UserProfileModel>> GetFollowing(int userId, int? viewerId);

        Task<List<UserProfileModel>> GetBlocked(int userId, int? viewerId);

        Task MakeFirstUserDev(UserProfileModel user);

        Task<bool> HandleExists(string handle);
        
        Task<List<UserProfileModel>> GetAll(DateTime? sinceLastUpdated);

        Task<LoginResult> LoginPublicKeyChallenge(int userId, PublicKey publicKey, PrivateKey privateKey, string challenge, string answer, IAuthTypeHandler authTypeHandler);
        
        Task<List<UserProfileModel>> GetCompletionsFor(string prefix, int viewerId);
    }
}