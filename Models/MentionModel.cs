using System;
using System.ComponentModel;

namespace PinkUmbrella.Models
{
    public class MentionModel
    {
        public int Id { get; set; }

        [DisplayName("When Mentioned"), Description("When the user was mentioned")]
        public DateTime WhenMentioned { get; set; }

        public int PostId { get; set; }
        
        public int MentionedUserId { get; set; }

        [DefaultValue(null)]
        public DateTime? WhenMentionedUserSeenMention { get; set; }
        
        [DefaultValue(null)]
        public DateTime? WhenMentionedUserDismissMention { get; set; }


        public UserProfileModel MentionedUser { get; set; }

        public PostModel Post { get; set; }
    }
}