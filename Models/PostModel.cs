using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Models.Public;

namespace PinkUmbrella.Models
{
    [DisplayName("Post"), Description("A post a user types or uploads.")]
    public class PostModel: IHazPublicId
    {
        [NotMapped]
        public long PeerId { get; set; }

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

        [DefaultValue(0), PersonalData, DisplayName("Block Count"), Description("How many times your post was blocked by other users.")]
        public int BlockCount { get; set; }

        [StringLength(1000), PersonalData, Description("The content of the post")]
        public string Content { get; set; }

        [DefaultValue(0)]
        public int NextInChain { get; set; }


        [NotMapped, Nest.Ignore]
        public PublicProfileModel User { get; set; }

        [NotMapped, Nest.Ignore]
        public List<MentionModel> Mentions { get; set; } = new List<MentionModel>();

        [NotMapped, Nest.Ignore]
        public List<TagModel> Tags { get; set; } = new List<TagModel>();

        [NotMapped, JsonPropertyName("tags"), Nest.PropertyName("tags")]
        public string[] TagStrings
        {
            get
            {
                return Tags.Select(t => t.Tag).ToArray();
            }
            set
            {
                Tags = value.Select(t => new TagModel() { Tag = t }).ToList();
            }
        }


        [NotMapped, Nest.Ignore]
        public bool HasLiked { get; set; }
        
        [NotMapped, Nest.Ignore]
        public bool HasDisliked { get; set; }
        
        [NotMapped, Nest.Ignore]
        public bool HasBlocked { get; set; }

        [NotMapped, Nest.Ignore]
        public bool HasReported { get; set; }


        [NotMapped, Nest.Ignore]
        public int? ViewerId { get; set; }

        [NotMapped, Nest.Ignore]
        public bool HasBeenBlockedOrReported { get; set; }
        
        [NotMapped, Nest.Ignore]
        public List<ReactionModel> Reactions { get; set; } = new List<ReactionModel>();

        [NotMapped, Nest.Ignore]
        public bool ViewerIsFollowing { get; set; }

        [NotMapped, Nest.Ignore]
        public bool ViewerIsPoster => ViewerId.HasValue && UserId == ViewerId.Value;

        [NotMapped, Nest.Ignore]
        public PublicId PublicId => new PublicId(Id, PeerId);
    }
}