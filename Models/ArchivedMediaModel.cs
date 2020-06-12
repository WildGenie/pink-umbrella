using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace seattle.Models
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
        
        public int SizeBytes { get; set; }

        [MinLength(4), MaxLength(500)]
        public string Path { get; set; }
        
        public int UserId { get; set; }

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
    }
}