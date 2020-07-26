using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Tides.Models;
using Tides.Util;

namespace Tides.Core
{
    [IsDocumented]
    public class BaseObject: BaseObjectReference, IHazReactions
    {
        public BaseObject(string type = null)
        {
            this.type = string.IsNullOrEmpty(type) ? "Object" : type;
        }

        public string id { get; set; }
        public CollectionObject attachment { get; set; }
        public CollectionObject attributedTo { get; set; }
        public CollectionObject audience { get; set; }
        public string content { get; set; }
        public Visibility? visibility { get; set; }
        public CollectionObject context { get; set; }
        public string name { get; set; }
        public DateTime? endTime { get; set; }
        public BaseObject generator { get; set; }
        public CollectionObject icon { get; set; }
        public CollectionObject image { get; set; }
        public CollectionObject inReplyTo { get; set; }
        public CollectionObject location { get; set; }
        public BaseObject preview { get; set; }
        public DateTime? published { get; set; }
        public BaseObject publisher { get; set; }
        public CollectionObject replies { get; set; }
        public DateTime? startTime { get; set; }
        public string summary { get; set; }
        public CollectionObject tag { get; set; }
        public DateTime? updated { get; set; }
        public DateTime? deleted { get; set; }
        public BaseObject deletedBy { get; set; }
        public BaseObject url { get; set; }
        public CollectionObject to { get; set; }
        public CollectionObject bto { get; set; }
        public CollectionObject cc { get; set; }
        public CollectionObject bcc { get; set; }
        public string mediaType { get; set; }
        public TimeSpan? duration { get; set; }

        
        [JsonPropertyName("isMature"), DisplayName("Is Mature"), Description("If the post is for mature audiences")]
        public bool IsMature { get; set; }


        [JsonPropertyName("isActor")]
        public bool IsActor => Array.IndexOf(new string[]{"Actor", "Person", "Organization", ""}, type) >= 0;

        [JsonPropertyName("userId")]
        public int UserId => objectId.HasValue && IsActor ? objectId.Value : throw new Exception();


        [JsonIgnore]
        public string DisplayName => name;

        [JsonIgnore]
        public string Bio => summary;


        [NotMapped, Nest.Ignore]
        public List<int> FollowerIds { get; set; }



        public int LikeCount { get; set; }

        public int DislikeCount { get; set; }

        public int ReportCount { get; set; }

        public int BlockCount { get; set; }

        public int FollowCount { get; set; }

        public bool HasLiked { get; set; }
        
        public bool HasDisliked { get; set; }

        public bool HasFollowed { get; set; }
        
        public bool HasBlocked { get; set; }

        public bool HasReported { get; set; }

        public bool HasBeenBlockedOrReportedByViewer { get; set; }
        public bool HasBeenBlockedOrReportedByPublisher { get; set; }
        
        public OrderedCollectionObject Reactions { get; set; }


        [NotMapped, JsonIgnore, Nest.Ignore]
        public int? ViewerId { get; set; }

        [NotMapped, JsonIgnore, Nest.Ignore]
        public bool ViewerIsFollowing { get; set; }

        [NotMapped, JsonIgnore, Nest.Ignore]
        public bool ViewerIsPublisher => ViewerId.HasValue && UserId == ViewerId.Value;

        [NotMapped, JsonIgnore, Nest.Ignore]
        public bool IsReply => inReplyTo != null && (inReplyTo.totalItems > 0 || inReplyTo.items.Count > 0);

        [NotMapped, JsonIgnore, Nest.Ignore]
        public bool ShadowBanned
        {
            get
            {
                return visibility == Visibility.SHADOW_BANNED;
            }
            set
            {
                visibility = Visibility.SHADOW_BANNED;
            }
        }
    }
}