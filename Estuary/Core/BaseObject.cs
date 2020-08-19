using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Tides.Models;
using Tides.Models.Public;
using Tides.Util;

namespace Estuary.Core
{
    // [Description("Visibility of this to other users.")]
    [IsDocumented]
    public class BaseObject: BaseObjectReference, IHazReactions
    {
        public BaseObject(string type, string baseType)
        {
            this.type = string.IsNullOrEmpty(type) ? "Object" : type;
            BaseType = string.IsNullOrEmpty(baseType) ? "Object" : baseType;
        }

        public BaseObject() { }

        public int? statusCode { get; set; }

        [NotMapped]
        public CollectionObject attachment { get; set; }
        
        [NotMapped]
        public CollectionObject attributedTo { get; set; }
        
        [NotMapped]
        public CollectionObject audience { get; set; }
        
        public string content { get; set; }
        
        [NotMapped]
        public CollectionObject context { get; set; }
        
        public string name { get; set; }
        
        public DateTime? endTime { get; set; }
        
        [NotMapped]
        public BaseObject generator { get; set; }
        
        [NotMapped]
        public CollectionObject icon { get; set; }
        
        [NotMapped]
        public CollectionObject image { get; set; }
        
        [NotMapped]
        public CollectionObject inReplyTo { get; set; }
        
        [NotMapped]
        public CollectionObject location { get; set; }
        
        [NotMapped]
        public BaseObject preview { get; set; }
        
        public DateTime? published { get; set; }
        
        [NotMapped]
        public CollectionObject replies { get; set; }
        
        public DateTime? startTime { get; set; }
        
        public string summary { get; set; }
        
        [JsonPropertyName("tag")]
        public CollectionObject tag { get; set; }

        [DisplayName("When Last Updated"), Description("When this was last updated.")]
        public DateTime? updated { get; set; }
        
        public DateTime? deleted { get; set; }
        
        [NotMapped]
        public BaseObject deletedBy { get; set; }
        
        [NotMapped]
        public BaseObject url { get; set; }
        
        [NotMapped]
        public CollectionObject from { get; set; }
        
        [NotMapped]
        public CollectionObject to { get; set; }
        
        [NotMapped]
        public CollectionObject bto { get; set; }
        
        [NotMapped]
        public CollectionObject cc { get; set; }
        
        [NotMapped]
        public CollectionObject bcc { get; set; }
        
        public string mediaType { get; set; }
        
        public TimeSpan? duration { get; set; }

        
        [JsonPropertyName("isMature"), DisplayName("Is Mature"), Description("If the post is for mature audiences")]
        public bool? IsMature { get; set; }


        [JsonIgnore]
        public string DisplayName => name;

        [JsonIgnore]
        public string Bio => summary;


        [NotMapped, Nest.Ignore]
        public List<int> FollowerIds { get; set; }



        [JsonIgnore]
        public int LikeCount { get; set; }

        [JsonIgnore]
        public int DislikeCount { get; set; }

        [JsonIgnore]
        public int UpvoteCount { get; set; }

        [JsonIgnore]
        public int DownvoteCount { get; set; }

        [JsonIgnore]
        public int IgnoreCount { get; set; }

        [JsonIgnore]
        public int ReportCount { get; set; }

        [JsonIgnore]
        public int BlockCount { get; set; }

        [JsonIgnore]
        public int FollowCount { get; set; }

        [JsonIgnore]
        public bool HasLiked { get; set; }
        
        [JsonIgnore]
        public bool HasDisliked { get; set; }
        
        [JsonIgnore]
        public bool HasUpvoted { get; set; }
        
        [JsonIgnore]
        public bool HasDownvoted { get; set; }

        [JsonIgnore]
        public bool HasFollowed { get; set; }
        
        [JsonIgnore]
        public bool HasIgnored { get; set; }
        
        [JsonIgnore]
        public bool HasBlocked { get; set; }

        [JsonIgnore]
        public bool HasReported { get; set; }

        [JsonIgnore]
        public bool HasBeenBlockedOrReportedByViewer { get; set; }

        [JsonIgnore]
        public bool HasBeenBlockedOrReportedByPublisher { get; set; }
        
        [NotMapped]
        public CollectionObject Reactions { get; set; }


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
                return statusCode == 403;
            }
            set
            {
                statusCode = 403;
            }
        }

        //[JsonIgnore]
        //public override PublicId PublicId => base.PublicId ?? (string.IsNullOrWhiteSpace(id) ? null : new PublicId(id));

        [JsonIgnore]
        public bool IsBaseObjectDefined =>
                        objectId != null ||
                        !string.IsNullOrWhiteSpace(id) || !string.IsNullOrWhiteSpace(content) ||
                        !string.IsNullOrWhiteSpace(summary) || !string.IsNullOrWhiteSpace(name) ||
                        !string.IsNullOrWhiteSpace(mediaType) || // !string.IsNullOrWhiteSpace(type) || 
                        (attachment != null && attachment.IsDefined) || (attributedTo != null && attributedTo.IsDefined) || 
                        (audience != null && audience.IsDefined) || (context != null && context.IsDefined) || 
                        (generator != null && generator.IsDefined) || (icon != null && icon.IsDefined) || 
                        (image != null && image.IsDefined) || (inReplyTo != null && inReplyTo.IsDefined) || 
                        (location != null && location.IsDefined) || (preview != null && preview.IsDefined) || 
                        (replies != null && replies.IsDefined) || (tag != null && tag.IsDefined) || 
                        (deletedBy != null && deletedBy.IsDefined) || (url != null && url.IsDefined) || 
                        (from != null && from.IsDefined) ||
                        (to != null && to.IsDefined) || (bto != null && bto.IsDefined) || 
                        (cc != null && cc.IsDefined) || (bcc != null && bcc.IsDefined) || 
                        
                        endTime.HasValue || published.HasValue ||
                        startTime.HasValue || duration.HasValue || updated.HasValue ||
                        deleted.HasValue;

        [JsonIgnore]
        public virtual bool IsDefined => IsBaseObjectDefined;
    }
}