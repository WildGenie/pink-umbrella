using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkUmbrella.Models
{
    public class TagModel
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Tag { get; set; }
        
        [DefaultValue(false), DisplayName("Contains Profanity"), Description("If the tag contains profanity")]
        public bool ContainsProfanity { get; set; }

        [DefaultValue(0), DisplayName("Like Count"), Description("How many times your post was liked by other users.")]
        public int LikeCount { get; set; }

        [DefaultValue(0), DisplayName("Dislike Count"), Description("How many times your post was disliked by other users.")]
        public int DislikeCount { get; set; }

        [DefaultValue(0), DisplayName("Block Count"), Description("How many times your post was blocked by other users.")]
        public int BlockCount { get; set; }

        [DisplayName("Created By User Id"), Description("Who first used this tag.")]
        public int CreatedByUserId { get; set; }

        // Bind to constant topic list, otherwise it is a general topic
        [NotMapped]
        public string Topic { get; set; }


        [NotMapped]
        public bool HasLiked { get; set; }
        
        [NotMapped]
        public bool HasDisliked { get; set; }
        
        [NotMapped]
        public bool HasBlocked { get; set; }

        [NotMapped]
        public long UseCount { get; set; }
    }
}