using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace seattle.Models
{
    [DisplayName("Post"), Description("A post a user types or uploads.")]
    public class PostModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [DefaultValue(Visibility.VISIBLE_TO_REGISTERED)]
        public Visibility Visibility { get; set; }

        public PostType PostType { get; set; }
        public bool IsReply { get; set; }

        [PersonalData, DisplayName("When Posted"), Description("When the post was made")]
        public DateTime WhenCreated { get; set; }

        [DefaultValue(null), PersonalData, DisplayName("When Deleted"), Description("When the post was deleted")]
        public DateTime? WhenDeleted { get; set; }

        [DefaultValue(null), PersonalData, DisplayName("Deleted By Who"), Description("Who deleted the post")]
        public int? DeletedByUserId { get; set; }
        
        [DefaultValue(false), PersonalData, DisplayName("Contains Profanity"), Description("If the post contains profanity")]
        public bool ContainsProfanity { get; set; }

        [DefaultValue(false)]
        public bool ShadowBanned { get; set; }

        [DefaultValue(0), PersonalData, DisplayName("Like Count"), Description("How many times your post was liked by other users.")]
        public int LikeCount { get; set; }

        [DefaultValue(0), PersonalData, DisplayName("Dislike Count"), Description("How many times your post was disliked by other users.")]
        public int DislikeCount { get; set; }

        [DefaultValue(0), PersonalData, DisplayName("Report Count"), Description("How many times your post was reported by other users.")]
        public int ReportCount { get; set; }

        [StringLength(1000), PersonalData, Description("The content of the post")]
        public string Content { get; set; }

        [DefaultValue(0)]
        public int NextInChain { get; set; }


        //[ForeignKey("UserId")]
        public UserProfileModel User { get; set; }

        [NotMapped]
        public List<MentionModel> Mentions { get; set; } = new List<MentionModel>();
    }
}