using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using PinkUmbrella.Models.Public;
using PinkUmbrella.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class MentionModel
    {
        public int Id { get; set; }

        [DisplayName("When Mentioned"), Description("When the user was mentioned")]
        public DateTime WhenMentioned { get; set; }

        public int PostId { get; set; }

        [NotMapped]
        public long PostPeerId { get; set; }
        
        public int MentionedUserId { get; set; }

        public long MentionedUserPeerId { get; set; }

        [DefaultValue(null)]
        public DateTime? WhenMentionedUserSeenMention { get; set; }
        
        [DefaultValue(null)]
        public DateTime? WhenMentionedUserDismissMention { get; set; }

        public UserProfileModel MentionedUser { get; set; }

        public PostModel Post { get; set; }



        [NotMapped]
        public PublicProfileModel MentionedPublicUser { get; set; }
    }
}