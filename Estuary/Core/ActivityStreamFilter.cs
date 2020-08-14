using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Estuary.Objects;
using Tides.Models.Public;

namespace Estuary.Core
{
    public class ActivityStreamFilter
    {
        public ActivityStreamFilter(string index)
        {
            this.index = string.IsNullOrWhiteSpace(index) ? throw new ArgumentNullException(index) : index;
        }
        
        public string index { get; }

        public int? sinceId { get; set; }

        public bool? countOnly { get; set; }

        public int? viewerId { get; set; }
        
        public long? peerId { get; set; }
        public int? userId { get; set; }
        public int? objectId { get; set; }
        public string handle { get; set; }
        public string[] types { get; set; }
        public string[] objectTypes { get; set; }
        public string[] targetTypes { get; set; }
        public CommonMediaType? type { get; set; }
        public DateTime? sinceLastUpdated { get; set; }
        public bool? includeReplies { get; set; }

        public bool reverse { get; set; } = true;

        public string[] order { get; set; }
        

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

        public string id { get; set; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this);

        public override string ToString()
        {
            var uriBuilder = new UriBuilder("activity", "localhost", 0, index);
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            
            if (!string.IsNullOrWhiteSpace(id))
            {
                queryString.Add(nameof(id), id);
            }
            else if (publicId != null)
            {
                queryString.Add(nameof(id), publicId.ToString());
            }

            if (types != null && types.Length > 0)
            {
                foreach (var t in types)
                {
                    queryString.Add($"{nameof(types)}[]", t);
                }
            }

            uriBuilder.Query = queryString.ToString();
            return uriBuilder.ToString();
        }

        public bool IsMatch(BaseObject obj)
        {
            if (includeReplies.HasValue)
            {
                if (includeReplies.Value && !obj.IsReply)
                {
                    return false;
                }
                else if (!includeReplies.Value && obj.IsReply)
                {
                    return false;
                }
            }
            if (types != null && (!types.Contains(obj.type) && !types.Contains(obj.BaseType)))
            {
                return false;
            }
            if (obj is ActivityObject activity)
            {
                if (objectTypes != null && (activity.obj == null || (!objectTypes.Contains(activity.obj.type) && !objectTypes.Contains(obj.BaseType))))
                {
                    return false;
                }
            }
            return true;
        }

        public string ToUrlFormEncoded()
        {
            var pairs = this.GetType().GetProperties()
                .Select(p => $"{p.Name}={Uri.EscapeDataString(p.GetValue(this)?.ToString() ?? "")}")
                .ToArray();
            return string.Join('&', pairs);
        }
    }
}