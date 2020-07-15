using System;
using System.Text.Json.Serialization;

namespace PinkUmbrella.Models.Public
{
    public class PublicId
    {
        public PublicId() { }

        public PublicId(int id, long peerId)
        {
            Id = id;
            PeerId = peerId;
        }

        public PublicId(string id)
        {
            var split = (id ?? throw new ArgumentNullException(nameof(id))).Split('-');
            if (split.Length == 2)
            {
                PeerId = long.Parse(split[0]);
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

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("peerId")]
        public long PeerId { get; set; }

        //override public string ToString() => PeerId == 0 ? Id.ToString() : $"{PeerId}-{Id}";
        override public string ToString() => $"{PeerId}-{Id}";
    }
}