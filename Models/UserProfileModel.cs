using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PinkUmbrella.Models
{
    [DisplayName("User Profile"), Description("Profile data for each user.")]
    public class UserProfileModel : IdentityUser<int>
    {
        [PersonalData, DefaultValue(Visibility.VISIBLE_TO_WORLD), Description("Visibility of your profile to other users.")]
        public Visibility Visibility { get; set; }

        [PersonalData, DisplayName("Joined"), Description("When you joined this site.")]
        public DateTime WhenCreated { get; set; }

        [PersonalData, DisplayName("Deleted"), DefaultValue(null), Description("When your profile was deleted.")]
        public DateTime? WhenDeleted { get; set; }

        [PersonalData, DisplayName("WhenLastUpdated"), DefaultValue(0), Description("When your profile was last updated.")]
        public DateTime WhenLastUpdated { get; set; }

        [PersonalData, DisplayName("Deleted By"), DefaultValue(null), Description("Who your profile was deleted by.")]
        public int? DeletedByUserId { get; set; }

        [PersonalData, DisplayName("When Last Logged In"), DefaultValue(0), Description("The exact time and date when you last logged in.")]
        public DateTime WhenLastLoggedIn { get; set; }

        [PersonalData, DisplayName("When Last Logged In Visibility"), DefaultValue(Visibility.VISIBLE_TO_WORLD), Description("Visibility of when you last logged in to other users.")]
        public Visibility WhenLastLoggedInVisibility { get; set; }

        [PersonalData, DisplayName("When Last Online"), Description("The exact time and date when you last connected to the site.")]
        public DateTime WhenLastOnline { get; set; }

        [PersonalData, DisplayName("When Last Online Visibility"), DefaultValue(Visibility.VISIBLE_TO_WORLD), Description("Visibility of when you last connected to other users.")]
        public Visibility WhenLastOnlineVisibility { get; set; }

        [PersonalData, StringLength(30, MinimumLength = 3), Required, Description("An identifiable handle to easily reference your profile.")]
        [Remote("IsHandleUnique", "Account",  HttpMethod = "GET", ErrorMessage = "Handle already in use.")]
        [RegularExpression(@"[a-zA-Z0-9_]{3,30}", ErrorMessage = "Handle must be between 3 and 30 characters, and only contain letters, numbers, and underscores.")]
        public string Handle { get; set; }

        [PersonalData, DisplayName("Display Name"), StringLength(50), Required, Description("The name displayed to other users.")]
        public string DisplayName { get; set; }

        [PersonalData, StringLength(1000), Description("A biography of your profile shown to other users.")]
        public string Bio { get; set; }

        [PersonalData, DisplayName("Bio Visibility"), DefaultValue(Visibility.VISIBLE_TO_WORLD), Description("Visibility of your bio to other users.")]
        public Visibility BioVisibility { get; set; }


        [PersonalData, DefaultValue("{}"), StringLength(1000), JsonIgnore, DisplayName("External Profiles"), Description("What other social medias you use.")]
        public string ExternalUsernamesJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> ExternalUsernames { get; set; }

        [PersonalData, DefaultValue("{}"), StringLength(1000), JsonIgnore, DisplayName("External Profile Visibility"), Description("Visibility of your external social media profiles to other users.")]
        public string ExternalUsernameVisibilitiesJson { get; set; }

        [NotMapped]
        public Dictionary<string, Visibility> ExternalUsernameVisibilities { get; set; }
        

        [DefaultValue(null), DisplayName("You Are Allowed Back On"), Description("When your ban expires.")]
        public DateTime? BanExpires { get; set; }

        [PersonalData, DisplayName("Ban Reason"), StringLength(1000), DefaultValue("You Were Banned Because"), Description("The reasoning behind your ban.")]
        public string BanReason { get; set; }

        [DefaultValue(null), DisplayName("You Were Banned By User Id"), Description("Who you were banned by.")]
        public int? BannedByUserId { get; set; }

        [DefaultValue(0), DisplayName("Like Count"), Description("How many times your profile was liked by other users.")]
        public int LikeCount { get; set; }

        [DefaultValue(0), DisplayName("Dislike Count"), Description("How many times your profile was disliked by other users.")]
        public int DislikeCount { get; set; }

        [DefaultValue(0), DisplayName("Report Count"), Description("How many times your profile was reported by other users.")]
        public int ReportCount { get; set; }

        [DefaultValue(0), DisplayName("Block Count"), Description("How many times your profile was blocked by other users.")]
        public int BlockCount { get; set; }

        [DefaultValue(0), DisplayName("Follow Count"), Description("How many times your profile was followed by other users.")]
        public int FollowCount { get; set; }

        
        [NotMapped]
        public List<ReactionModel> Reactions { get; set; } = new List<ReactionModel>();

        [NotMapped]
        public List<int> FollowerIds { get; set; }


        [NotMapped]
        public bool HasLiked { get; set; }
        
        [NotMapped]
        public bool HasDisliked { get; set; }

        [NotMapped]
        public bool HasFollowed { get; set; }
        
        [NotMapped]
        public bool HasBlocked { get; set; }

        [NotMapped]
        public bool HasReported { get; set; }

        [NotMapped]
        public bool HasBeenBlockedOrReported { get; set; }
    }
}