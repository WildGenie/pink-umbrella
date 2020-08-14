using System;
using System.Text.Json.Serialization;
using Tides.Models.Public;

namespace Estuary.Core
{
    public class BaseObjectReference: IHazPublicId
    {
        private PublicId _pubId = null;

        public string type
        {
            get => _pubId?.Type;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                else
                {
                    if (value == BaseType)
                    {
                        throw new ArgumentException($"Cannot have same type as base type");
                    }
                    EnsurePublicId().Type = value;
                }
            }
        }

        [JsonIgnore]
        public string BaseType { get; set; }

        public long? hash { get; set; }

        [JsonPropertyName("peerId")]
        public long? PeerId { get => PublicId?.PeerId; set => EnsurePublicId().PeerId = value ?? 0; }

        public int? objectId { get => PublicId?.Id; set => EnsurePublicId().Id = value ?? 0; }

        [JsonPropertyName("userId")]
        public virtual int? UserId => objectId.HasValue && IsActor ? objectId : PublicId?.UserId;
        
        [JsonIgnore]
        public bool IsActor => Array.IndexOf(new string[]{"Actor", "Person", "Organization"}, type) >= 0;
        
        [JsonIgnore]
        public virtual PublicId PublicId
        {
            get
            {
                return _pubId;
            }
            set
            {
                if (value == null)
                {
                    _pubId = null;
                }
                else
                {
                    EnsurePublicId().Copy(value);
                }
            }
        }

        public string id
        {
            get
            {
                return PublicId?.ToString();
            }
            set
            {
                var v = new PublicId(value);
                if (_pubId == null)
                {
                    _pubId = v;
                }
                else
                {
                    _pubId.Copy(v);
                }
            }
        }

        private PublicId EnsurePublicId()
        {
            if (_pubId == null)
            {
                _pubId = new PublicId();
            }
            return _pubId;
        }
    }
}