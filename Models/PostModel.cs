using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace seattle.Models
{
    public class PostModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [DefaultValue(Visibility.VISIBLE_TO_REGISTERED)]
        public Visibility Visibility { get; set; }

        public PostType PostType { get; set; }
        public bool IsReply { get; set; }

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

        [StringLength(1000)]
        public string Content { get; set; }

        [DefaultValue(0)]
        public int NextInChain { get; set; }
    }
}