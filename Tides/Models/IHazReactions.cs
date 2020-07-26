using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Tides.Core;

namespace Tides.Models
{
    public interface IHazReactions
    {
        [DefaultValue(0), DisplayName("Like Count"), Description("How many times this was liked by other users.")]
        int LikeCount { get; set; }

        [DefaultValue(0), DisplayName("Dislike Count"), Description("How many times this was disliked by other users.")]
        int DislikeCount { get; set; }

        [DefaultValue(0), DisplayName("Report Count"), Description("How many times this was reported by other users.")]
        int ReportCount { get; set; }

        [DefaultValue(0), DisplayName("Block Count"), Description("How many times this was blocked by other users.")]
        int BlockCount { get; set; }

        [DefaultValue(0), DisplayName("Follow Count"), Description("How many times this was followed by other users.")]
        int FollowCount { get; set; }

        [NotMapped, Nest.Ignore]
        bool HasLiked { get; set; }
        
        [NotMapped, Nest.Ignore]
        bool HasDisliked { get; set; }

        [NotMapped, Nest.Ignore]
        bool HasFollowed { get; set; }
        
        [NotMapped, Nest.Ignore]
        bool HasBlocked { get; set; }

        [NotMapped, Nest.Ignore]
        bool HasReported { get; set; }

        [NotMapped, Nest.Ignore]
        bool HasBeenBlockedOrReportedByViewer { get; set; }

        [NotMapped, Nest.Ignore]
        bool HasBeenBlockedOrReportedByPublisher { get; set; }



        
        [NotMapped, JsonIgnore, Nest.Ignore]
        OrderedCollectionObject Reactions { get; set; }
    }
}