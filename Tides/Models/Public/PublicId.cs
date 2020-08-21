using System;
using System.Text;
using System.Text.Json.Serialization;

namespace Tides.Models.Public
{
    public class PublicId
    {
        public PublicId() { }

        public PublicId(int? id, long? peerId)
        {
            Id = id;
            PeerId = peerId;
        }

        public PublicId(int userId, long peerId, int id)
        {
            Id = id;
            PeerId = peerId;
            UserId = userId;
        }

        public PublicId(Guid guid)
        {
            var bytes = guid.ToByteArray();
            PeerId = BitConverter.ToInt64(bytes);
            UserId = BitConverter.ToInt32(bytes, 8);
            Id = BitConverter.ToInt32(bytes, 12);
        }

        public PublicId(string id)
        {
            var typeSplit = (id ?? throw new ArgumentNullException(nameof(id))).Split('/');
            if (typeSplit.Length == 2)
            {
                Type = typeSplit[0];
                id = typeSplit[0] = typeSplit[1];
            }

            try
            {
                if (Guid.TryParse(id, out var guid))
                {
                    var bytes = guid.ToByteArray();
                    PeerId = BitConverter.ToInt64(bytes);
                    UserId = BitConverter.ToInt32(bytes, 8);
                    Id = BitConverter.ToInt32(bytes, 12);
                }
                else
                {
                    var split = id.Split('-');
                    if (split.Length == 3)
                    {
                        PeerId = long.Parse(split[0]);
                        UserId = int.Parse(split[1]);
                        Id = int.Parse(split[2]);
                    }
                    else if (split.Length == 2)
                    {
                        UserId = int.Parse(split[0]);
                        Id = int.Parse(split[1]);
                    }
                    else if (split.Length == 1)
                    {
                        PeerId = 0;
                        Id = int.Parse(split[0]);
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
            }
            catch (FormatException e)
            {
                throw new ArgumentException($"Public Id {id} has invalid format", e);
            }
        }

        public void Copy(PublicId value)
        {
            UserId = value.UserId;
            ObjectId = value.ObjectId;
            PeerId = value.PeerId;
            Type = value.Type ?? Type;
        }

        [JsonPropertyName("id")]
        public int? Id
        {
            get => ObjectId ?? UserId;
            set => ObjectId = value;
        }

        [JsonPropertyName("userId")]
        public int? UserId { get; set; }

        [JsonPropertyName("objectId")]
        public int? ObjectId { get; set; }

        [JsonPropertyName("peerId")]
        public long? PeerId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        public bool IsLocal => PeerId == 0;

        public bool IsGuid => UserId.HasValue && PeerId.HasValue && PeerId.Value > 0;

        override public string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Type))
            {
                sb.Append(Type);
                sb.Append('/');
            }

            if (IsGuid)
            {
                sb.Append(AsGuid().ToString());
            }
            else
            {
                if (UserId.HasValue)
                {
                    if (PeerId.HasValue)
                    {
                        sb.Append(PeerId);
                        sb.Append('-');
                    }
                    sb.Append(UserId.Value);
                    sb.Append('-');
                }
                if (Id.HasValue)
                {
                    sb.Append(Id.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
            return sb.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj is PublicId other)
            {
                return this.Type == other.Type && this.PeerId == other.PeerId &&
                        this.UserId == other.UserId && this.Id == other.Id;
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(PublicId lhs, PublicId rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PublicId lhs, PublicId rhs) => !(lhs == rhs);


        public Guid AsGuid()
        {
            if (IsGuid)
            {
                var bytes = new byte[16];
                Array.Copy(BitConverter.GetBytes(PeerId.Value), bytes, 8);
                Buffer.BlockCopy(BitConverter.GetBytes(PeerId.Value), 0, bytes, 8, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(PeerId.Value), 0, bytes, 12, 4);
                return new Guid(bytes);
            }
            return Guid.Empty;
        }
    }
}