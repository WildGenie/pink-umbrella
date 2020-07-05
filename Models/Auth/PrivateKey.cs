using System;
using System.Text.Json.Serialization;

namespace PinkUmbrella.Models.Auth
{
    public class PrivateKey
    {
        public long Id { get; set; }
        
        [JsonPropertyName("type")]
        public AuthType Type { get; set; }
        
        [JsonPropertyName("format")]
        public AuthKeyFormat Format { get; set; }
        
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("whenAdded")]
        public DateTime WhenAdded { get; set; }

        public long PublicKeyId { get; set; }

        public PublicKey PublicKey { get; set; }

        public override string ToString() => $"{Type} ({Format}): private";
    }
}