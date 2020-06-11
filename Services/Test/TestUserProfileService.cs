using System;
using System.Collections.Generic;
using seattle.Models;

namespace seattle.Services.Test
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
            ret.Id = NextId++;
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
                Id = NextId++,
                ProfileVisibility = Visibility.VISIBLE_TO_WORLD,
                WhenCreated = DateTime.UtcNow,
                WhenLastLoggedIn = DateTime.UtcNow,
                WhenLastLoggedInVisibility = Visibility.VISIBLE_TO_WORLD,
                WhenLastOnline = DateTime.UtcNow,
                WhenLastOnlineVisibility = Visibility.VISIBLE_TO_WORLD,
                BioVisibility = Visibility.VISIBLE_TO_WORLD,
                Bio = ""
            };
        }
        
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