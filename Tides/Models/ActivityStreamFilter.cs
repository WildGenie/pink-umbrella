using System;
using System.Text.Json.Serialization;
using Tides.Models.Public;
using Tides.Objects;

namespace Tides.Models
{
    public class ActivityStreamFilter
    {
        public int? viewerId { get; set; }
        
        public long? peerId { get; set; }
        public int? userId { get; set; }
        public int? objectId { get; set; }
        public string index { get; set; }
        public string id { get; set; }
        public string handle { get; set; }
        public string[] types { get; set; }
        public string[] targetTypes { get; set; }
        public CommonMediaType? type { get; set; }
        public DateTime? sinceLastUpdated { get; set; }
        public bool? includeReplies { get; set; }


        [JsonIgnore]
        public PublicId publicId
        {
            get
            {
                var id = objectId ?? userId;
                return id.HasValue && peerId.HasValue ? new PublicId(id.Value, peerId.Value) : null;
            }
            set
            {
                objectId = value.Id;
                peerId = value.PeerId;
            }
        }
    }
}