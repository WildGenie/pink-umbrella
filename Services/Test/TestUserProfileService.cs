using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.ViewModels.Account;

namespace PinkUmbrella.Services.Test
{
    public class TestUserProfileService: IUserProfileService
    {
        private static int NextId = 1;

        private static List<UserProfileModel> TestItems = null;

        public List<UserProfileModel> GetTestUsers() {
            if (TestItems == null) {
                TestItems = new List<UserProfileModel>();
                for (int i = 0; i < 1000; i++) {
                    TestItems.Add(RandomTestUser());
                }
            }
            return TestItems;
        }

        private UserProfileModel RandomTestUser()
        {
            var ret = NewUser();
            ret.Handle = RandomHandle();
            ret.DisplayName = RandomDisplayName();
            ret.Bio = RandomBio();
            return ret;
        }

        private string RandomBio()
        {
            return "Hello";
        }

        private string RandomDisplayName()
        {
            return "User " + NextId;
        }

        private string RandomHandle()
        {
            return Guid.NewGuid().ToString();
        }

        private UserProfileModel NewUser()
        {
            return new UserProfileModel() {
                Visibility = Visibility.VISIBLE_TO_WORLD,
                WhenCreated = DateTime.UtcNow,
                WhenLastLoggedIn = DateTime.UtcNow,
                WhenLastLoggedInVisibility = Visibility.VISIBLE_TO_WORLD,
                WhenLastOnline = DateTime.UtcNow,
                WhenLastOnlineVisibility = Visibility.VISIBLE_TO_WORLD,
                BioVisibility = Visibility.VISIBLE_TO_WORLD,
                Bio = ""
            };
        }

        public Task<List<UserProfileModel>> GetMostRecentlyCreatedUsers()
        {
            throw new NotImplementedException();
        }

        public Task<UserProfileModel> GetUser(int id, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfileModel> GetUser(string handle, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public UserProfileModel CreateUser(RegisterInputModel initial)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUser(int id, int by_user_id)
        {
            throw new NotImplementedException();
        }

        public Task LogIn(int id, string from)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserProfileModel>> GetFollowers(int userId, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserProfileModel>> GetFollowing(int userId, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserProfileModel>> GetBlocked(int userId, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public bool CanView(UserProfileModel user, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task BindReferences(UserProfileModel user, int? viewerId)
        {
            throw new NotImplementedException();
        }
    }
}