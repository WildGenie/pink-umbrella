using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PinkUmbrella.Models
{
    public class ReactionModel
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("toId")]
        public int ToId { get; set; }

        [JsonPropertyName("toPeerId")]
        public long ToPeerId { get; set; }

        [JsonIgnore, DefaultValue(null)]
        public DateTime? WhenDeliveredToPeer { get; set; }

        [JsonIgnore, DefaultValue(0)]
        public int DeliverToPeerTryCount { get; set; }

        [JsonPropertyName("whenReacted")]
        public DateTime WhenReacted { get; set; }

        [JsonPropertyName("type")]
        public ReactionType Type { get; set; }

        [NotMapped, JsonPropertyName("fromPeerId")]
        public long FromPeerId { get; set; }
    }
}