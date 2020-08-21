using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Estuary.Objects;
using Tides.Models.Public;
using static Estuary.Activities.Common;

namespace Estuary.Core
{
    public class ActivityStreamFilter
    {
        [NotMapped]
        public PublicId activityId { get; set; }

        [NotMapped]
        public PublicId targetId { get; set; }

        public ActivityStreamFilter(string index)
        {
            this.index = string.IsNullOrWhiteSpace(index) ? throw new ArgumentNullException(index) : index;
        }
        
        public string index { get; }

        public PublicId id { get; set; }

        public int? sinceId { get; set; }

        [NotMapped]
        public bool? countOnly { get; set; }


        [NotMapped]
        public int? viewerId { get; set; }
        
        // public long? peerId { get; set; }
        // public int? userId { get; set; }
        // public int? objectId { get; set; }
        public string handle { get; set; }
        public string[] types { get; set; }
        public string[] objectTypes { get; set; }
        public string[] targetTypes { get; set; }
        public CommonMediaType? type { get; set; }
        public DateTime? sinceLastUpdated { get; set; }
        public bool? includeReplies { get; set; }

        [NotMapped]
        public bool reverse { get; set; } = true;
        
        [NotMapped]
        public bool performUndos { get; set; } = true;

        public string[] order { get; set; }
        

        // [JsonIgnore]
        // public PublicId publicId { get; set; }
        // {
        //     get
        //     {
        //         var id = objectId ?? userId;
        //         return id.HasValue && peerId.HasValue ? new PublicId(id.Value, peerId.Value) : null;
        //     }
        //     set
        //     {
        //         objectId = value.Id;
        //         peerId = value.PeerId;
        //     }
        // }

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

            if (id != null)
            {
                queryString.Add(nameof(id), id.ToString());
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
            if (obj.type == "Undo")
            {
                return true;
            }
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
            if (types != null)
            {
                if (obj is Undo undo)
                {
                    if (!types.Contains(undo.obj.type) && !types.Contains(undo.obj.BaseType) && !types.Contains(obj.type))
                    {
                        return false;
                    }
                }
                else if (!types.Contains(obj.type) && !types.Contains(obj.BaseType))
                {
                    return false;
                }
            }

            if (obj is ActivityObject activity)
            {
                if (objectTypes != null && (activity.obj == null || (!objectTypes.Contains(activity.obj.type) && !objectTypes.Contains(obj.BaseType))))
                {
                    return false;
                }
                
                if (activityId != null && activityId != activity.PublicId)
                {
                    return false;
                }
                if (targetId != null)
                {
                    if (activity.target != null && activity.target.items.Any(t => t.PublicId == targetId))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool Contains(ActivityStreamFilter other)
        {
            return (includeReplies == other.includeReplies || !includeReplies.HasValue) &&
                    (index == other.index || string.IsNullOrWhiteSpace(index)) &&
                    (id == other.id || id == null) &&
                    (handle == other.handle || string.IsNullOrWhiteSpace(handle)) &&
                    (types == null || other.types == null || !types.Except(other.types).Any()) &&
                    (objectTypes == null || other.objectTypes == null || !objectTypes.Except(other.objectTypes).Any()) &&
                    (targetTypes == null || other.targetTypes == null || !targetTypes.Except(other.targetTypes).Any()) &&
                    (type == null || type == other.type);
        }

        public ActivityStreamFilter Extend(ActivityStreamFilter other)
        {
            return new ActivityStreamFilter(index ?? other.index)
            {
                id = id ?? other.id,
                handle = handle ?? other.handle,
                types = types ?? other.types,
                objectTypes = objectTypes ?? other.objectTypes,
                targetTypes = targetTypes ?? other.targetTypes,
                type = type ?? other.type,
                // includeReplies = includeReplies ?? other.includeReplies,
            };
        }

        public string ToPath(PublicId focus)
        {
            var str = string.Empty;

            if (id != null)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    str += "/";
                }
                str += id;
            }

            if (!string.IsNullOrWhiteSpace(index))
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    str += "/";
                }
                str += index;
            }

            if (focus != null)
            {
                if (focus.IsGuid)
                {
                    return $"{str}/{focus.AsGuid()}.json";
                }
                else if (focus.PeerId.HasValue)
                {
                    return $"{str}/{focus.PeerId}-{focus.Id}.json";
                }
                else
                {
                    return $"{str}/{focus.Id}.json";
                }
            }
            else
            {
                return $"{str}.index";
            }
        }

        public string ToUrlFormEncoded()
        {
            var pairs = this.GetType().GetProperties()
                .Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute)) && p.GetValue(this) != null)
                .ToDictionary(k => k.Name, p =>
                {
                    var val = p.GetValue(this);
                    if (val is string[] stringArray)
                    {
                        return string.Join(',', stringArray);
                    }
                    else
                    {
                        return val.ToString();
                    }
                });
            
            pairs.Remove(nameof(id));
            pairs.Remove(nameof(index));
            pairs.Remove(nameof(reverse));
            
            return string.Join('&', pairs.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        }

        public string ToUri(PublicId focus)
        {
            var str = ToPath(focus);
            var args = ToUrlFormEncoded();
            if (string.IsNullOrWhiteSpace(args))
            {
                return str;
            }
            else
            {
                return $"{System.IO.Path.ChangeExtension(str, null)}?{args}.index";
            }
        }
    }
}