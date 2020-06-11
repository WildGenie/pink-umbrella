using System;
using System.Collections.Generic;

namespace seattle.Models
{
    public class UserProfileModel
    {
        public int Id { get; set; }
        public Visibility ProfileVisibility { get; set; }

        public DateTime WhenCreated { get; set; }
        public DateTime WhenLastLoggedIn { get; set; }
        public Visibility WhenLastLoggedInVisibility { get; set; }
        public DateTime WhenLastOnline { get; set; }
        public Visibility WhenLastOnlineVisibility { get; set; }
        public string Handle { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public Visibility BioVisibility { get; set; }


        public string ExternalUsernamesJson { get; set; }
        public Dictionary<string, string> ExternalUsernames { get; set; }
        public string ExternalUsernameVisibilitiesJson { get; set; }
        public Dictionary<string, Visibility> ExternalUsernameVisibilities { get; set; }
    }
}