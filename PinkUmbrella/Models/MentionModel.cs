using System;
using System.ComponentModel;
using Tides.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class MentionModel
    {
        public int Id { get; set; }

        [DisplayName("When Mentioned"), Description("When the user was mentioned")]
        public DateTime WhenMentioned { get; set; }

        public int PostId { get; set; }
        
        public int MentionedUserId { get; set; }

        public long MentionedUserPeerId { get; set; }

        [DefaultValue(null)]
        public DateTime? WhenMentionedUserSeenMention { get; set; }
        
        [DefaultValue(null)]
        public DateTime? WhenMentionedUserDismissMention { get; set; }
    }
}