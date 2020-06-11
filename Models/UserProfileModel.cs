using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace seattle.Models
{
    public class UserProfileModel : IdentityUser<int>
    {
        [PersonalData, DefaultValue(Visibility.VISIBLE_TO_REGISTERED)]
        public Visibility Visibility { get; set; }

        [PersonalData, DisplayName("Joined")]
        public DateTime WhenCreated { get; set; }

        [PersonalData, DisplayName("Deleted"), DefaultValue(null)]
        public DateTime? WhenDeleted { get; set; }

        [PersonalData, DisplayName("Deleted By"), DefaultValue(null)]
        public int? DeletedByUserId { get; set; }

        [PersonalData, DisplayName("When Last Logged In"), DefaultValue(0)]
        public DateTime WhenLastLoggedIn { get; set; }

        [PersonalData, DisplayName("When Last Logged In Visibility"), DefaultValue(Visibility.VISIBLE_TO_FOLLOWING)]
        public Visibility WhenLastLoggedInVisibility { get; set; }

        [PersonalData, DisplayName("When Last Online")]
        public DateTime WhenLastOnline { get; set; }

        [PersonalData, DisplayName("When Last Online Visibility"), DefaultValue(Visibility.VISIBLE_TO_FOLLOWING)]
        public Visibility WhenLastOnlineVisibility { get; set; }

        [PersonalData, StringLength(30), Required]
        public string Handle { get; set; }

        [PersonalData, DisplayName("Display Name"), StringLength(30), Required]
        public string DisplayName { get; set; }

        [PersonalData, StringLength(1000)]
        public string Bio { get; set; }

        [PersonalData, DisplayName("Bio Visibility"), DefaultValue(Visibility.VISIBLE_TO_REGISTERED)]
        public Visibility BioVisibility { get; set; }


        [PersonalData, DefaultValue("{}"), StringLength(1000), JsonIgnore]
        public string ExternalUsernamesJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> ExternalUsernames { get; set; }

        [PersonalData, DefaultValue("{}"), StringLength(1000), JsonIgnore]
        public string ExternalUsernameVisibilitiesJson { get; set; }

        [NotMapped]
        public Dictionary<string, Visibility> ExternalUsernameVisibilities { get; set; }
        

        [DefaultValue(null), DisplayName("You Are Allowed Back On")]
        public DateTime? BanExpires { get; set; }

        [PersonalData, StringLength(1000), DefaultValue("You Were Banned Because")]
        public string BanReason { get; set; }

        [DefaultValue(null), DisplayName("You Were Banned By User Id")]
        public int? BannedByUserId { get; set; }
    }
}