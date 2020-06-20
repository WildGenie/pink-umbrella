using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Util;

namespace PinkUmbrella.Models
{
    [DisplayName("Business Page"), Description("A virtual storefront for local businesses.")]
    public class ShopModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [DefaultValue(Visibility.VISIBLE_TO_WORLD), PersonalData, Description("Visibility of your shop to other users.")]
        public Visibility Visibility { get; set; } = Visibility.VISIBLE_TO_WORLD;

        [Required, StringLength(100), PersonalData, Description("An identifiable handle to easily reference your shop."), InputPlaceHolder("my_shop"), DebugValue("planet_express")]
        public string Handle { get; set; }

        [Required, StringLength(200), PersonalData, DisplayName("Display Name"), Description("The name of your shop displayed to other users."), InputPlaceHolder("My Shop"), DebugValue("Planet Express")]
        public string DisplayName { get; set; }

        [Required, StringLength(1000), PersonalData, Description("A description of your shop - what makes it special or unique."), InputPlaceHolder("We offer indoor and outdoor goods, sundries, gifts, and really anything you need."), DebugValue("We ship to anywhere in the multiverse")]
        public string Description { get; set; }

        [Required, StringLength(300), PersonalData, DisplayName("Street Address"), Description("The street address of your shop within the city."), InputPlaceHolder("305 Harrison St"), DebugValue("305 Harrison St")]
        public string StreetAddress { get; set; }

        [Required, StringLength(20), PersonalData, DisplayName("Zip Code"), Description("The zip code of your shop within the city."), InputPlaceHolder("98109"), DebugValue("98109")]
        public string ZipCode { get; set; }

        [Required, StringLength(300), PersonalData, DisplayName("Website Link"), Description("Link to the website for your shop, if you have one."), InputPlaceHolder("https://yourwebsite.com"), DebugValue("https://www.google.com")]
        public string WebsiteLink { get; set; }

        [Required, StringLength(300), PersonalData, DisplayName("Menu Link"), Description("Link to a menu for your shop, if applicable."), InputPlaceHolder("https://yourwebsite.com/menu"), DebugValue("https://www.youtube.com")]
        public string MenuLink { get; set; }

        // Not currently used
        public int GeoLocationId { get; set; }

        [NotMapped]
        public GeoLocationModel GeoLocation { get; set; }


        [DisplayName("When Created"), Description("When the shop page was created on this site.")]
        public DateTime WhenCreated { get; set; }

        [DisplayName("When Deleted"), Description("When the shop page was deleted."), DefaultValue(null)]
        public DateTime? WhenDeleted { get; set; }
        
        [DisplayName("Last Updated"), Description("When the shop page was last updated.")]
        public DateTime LastUpdated { get; set; }
        
        [DefaultValue(0), DisplayName("Like Count"), Description("How many times your shop was liked by other users.")]
        public int LikeCount { get; set; }

        [DefaultValue(0), DisplayName("Dislike Count"), Description("How many times your shop was disliked by other users.")]
        public int DislikeCount { get; set; }

        [DefaultValue(0), DisplayName("Report Count"), Description("How many times your shop was reported by other users.")]
        public int ReportCount { get; set; }

        [PersonalData, DefaultValue("{}"), StringLength(1000), JsonIgnore, DisplayName("External Profiles"), Description("What other social medias your shop is on.")]
        public string ExternalUsernamesJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> ExternalUsernames { get; set; }





        [NotMapped]
        public UserProfileModel OwnerUser { get; set; }


        [NotMapped]
        public bool HasLiked { get; set; }
        
        [NotMapped]
        public bool HasDisliked { get; set; }

        [NotMapped]
        public bool HasReported { get; set; }


        [NotMapped]
        public int? ViewerId { get; set; }

        [NotMapped]
        public bool HasBeenBlockedOrReported { get; set; }
        
        [NotMapped]
        public List<ReactionModel> Reactions { get; set; } = new List<ReactionModel>();

        [NotMapped, Description("Make your business easier for other users to find.")]
        public List<TagModel> Tags { get; set; } = new List<TagModel>();

        [NotMapped]
        public bool ViewerIsOwner => ViewerId.HasValue && UserId == ViewerId.Value;
    }
}