using System.ComponentModel.DataAnnotations.Schema;
using Estuary.Core;
using Estuary.Util;
using Tides.Models;

namespace PinkUmbrella.Models
{
    public class ReactionsSummaryModel : IHazReactions, IHazComputedId
    {
        public string ComputedId => $"{ObjectType}-{ToId}-{ToPeerId}";
        public string ObjectType { get; set; }
        public int ToId { get; set; }
        public int ToPeerId { get; set; }

        // Redis Values
        [RedisValueAttribute]
        public int LikeCount { get; set; }
        
        [RedisValueAttribute]
        public int DislikeCount { get; set; }

        [RedisValueAttribute]
        public int UpvoteCount { get; set; }
        
        [RedisValueAttribute]
        public int DownvoteCount { get; set; }
        
        [RedisValueAttribute]
        public int ReportCount { get; set; }
        
        [RedisValueAttribute]
        public int BlockCount { get; set; }
        
        [RedisValueAttribute]
        public int IgnoreCount { get; set; }
        
        [RedisValueAttribute]
        public int FollowCount { get; set; }

        // NotMapped
        [NotMapped]
        public bool HasLiked { get; set; }
        
        [NotMapped]
        public bool HasDisliked { get; set; }

        [NotMapped]
        public bool HasUpvoted { get; set; }
        
        [NotMapped]
        public bool HasDownvoted { get; set; }
        
        [NotMapped]
        public bool HasFollowed { get; set; }
        
        [NotMapped]
        public bool HasBlocked { get; set; }
        
        [NotMapped]
        public bool HasReported { get; set; }
        
        [NotMapped]
        public bool HasBeenBlockedOrReportedByViewer { get; set; }
        
        [NotMapped]
        public bool HasBeenBlockedOrReportedByPublisher { get; set; }

        [NotMapped]
        public OrderedCollectionObject Reactions { get; set; }
    }
}