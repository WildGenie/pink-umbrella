using System;
using System.Text.Json.Serialization;

namespace PinkUmbrella.Models.Auth
{
    public class PublicKey
    {
        public long Id { get; set; }

        [JsonPropertyName("whenAdded")]
        public DateTime WhenAdded { get; set; }

        [JsonPropertyName("type")]
        public AuthType Type { get; set; }
        
        [JsonPropertyName("format")]
        public AuthKeyFormat Format { get; set; }

        [JsonPropertyName("fingerprint")]
        public string FingerPrint { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}