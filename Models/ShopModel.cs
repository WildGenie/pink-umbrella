using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace seattle.Models
{
    [DisplayName("Business Page"), Description("A virtual storefront for local businesses.")]
    public class ShopModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [DefaultValue(Visibility.VISIBLE_TO_WORLD), PersonalData, Description("Visibility of your shop to other users.")]
        public Visibility Visibility { get; set; }

        [Required, StringLength(100), PersonalData, Description("An identifiable handle to easily reference your shop.")]
        public string Handle { get; set; }

        [Required, StringLength(200), PersonalData, Description("The name of your shop displayed to other users.")]
        public string DisplayName { get; set; }

        public int GeoLocationId { get; set; }

        [NotMapped]
        public GeoLocationModel GeoLocation { get; set; }


        [PersonalData, DisplayName("When Created"), Description("When the shop page was created on this site.")]
        public DateTime WhenCreated { get; set; }
        
        [PersonalData, DisplayName("Last Updated"), Description("When the shop page was last updated.")]
        public DateTime LastUpdated { get; set; }
        
        [DefaultValue(0), PersonalData, DisplayName("Like Count"), Description("How many times your shop was liked by other users.")]
        public int LikeCount { get; set; }

        [DefaultValue(0), PersonalData, DisplayName("Dislike Count"), Description("How many times your shop was disliked by other users.")]
        public int DislikeCount { get; set; }

        [DefaultValue(0), PersonalData, DisplayName("Report Count"), Description("How many times your shop was reported by other users.")]
        public int ReportCount { get; set; }
    }
}