using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkUmbrella.Models
{
    public class ArchivedMediaModel
    {
        public int Id { get; set; }

        [DefaultValue(null), MaxLength(100)]
        public string OriginalName { get; set; }

        [DefaultValue(null), MaxLength(100)]
        public string DisplayName { get; set; }

        [DefaultValue(null), MaxLength(1000)]
        public string Description { get; set; }

        [DefaultValue(null), MaxLength(100)]
        public string Attribution { get; set; }
        
        public int SizeBytes { get; set; }

        [MinLength(4), MaxLength(500)]
        public string Path { get; set; }
        
        public int UserId { get; set; }

        [DefaultValue(null), ]
        public int? RelatedPostId { get; set; }

        [DefaultValue(Visibility.VISIBLE_TO_REGISTERED)]
        public Visibility Visibility { get; set; }

        public DateTime WhenCreated { get; set; }

        [DefaultValue(null)]
        public DateTime? WhenDeleted { get; set; }

        [DefaultValue(null)]
        public int? DeletedByUserId { get; set; }
        
        [DefaultValue(false)]
        public bool ContainsProfanity { get; set; }

        [DefaultValue(false)]
        public bool ShadowBanned { get; set; }

        [DefaultValue(0)]
        public int LikeCount { get; set; }

        [DefaultValue(0)]
        public int DislikeCount { get; set; }

        [DefaultValue(0)]
        public int ReportCount { get; set; }

        [DefaultValue(0)]
        public int BlockCount { get; set; }

        [DefaultValue(false)]
        public bool UploadedStatus { get; set; }

        public ArchivedMediaType MediaType { get; set; }


        public UserProfileModel User { get; set; }

        public PostModel RelatedPost { get; set; }


        [NotMapped]
        public bool HasLiked { get; set; }
        
        [NotMapped]
        public bool HasDisliked { get; set; }
        
        [NotMapped]
        public bool HasBlocked { get; set; }

        [NotMapped]
        public bool HasReported { get; set; }


        [NotMapped]
        public int? ViewerId { get; set; }

        [NotMapped]
        public bool HasBeenBlockedOrReported { get; set; }
        
        [NotMapped]
        public List<ReactionModel> Reactions { get; set; } = new List<ReactionModel>();

        [NotMapped]
        public bool ViewerIsFollowing { get; set; }

        [NotMapped]
        public bool ViewerIsPoster => ViewerId.HasValue && UserId == ViewerId.Value;
    }
}