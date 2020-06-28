using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Util;

namespace PinkUmbrella.Models.Peer
{
    public class PeerModelMetaData
    {
        
    }

    public class PeerModel
    {

        [JsonPropertyName("handle")]
        public string Handle { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("auth")]
        public AuthKey Auth { get; set; }

        [JsonIgnore]
        public PeerModelMetaData MetaData { get; set; }

        [JsonPropertyName("authId")]
        public long? AuthId { get; set; }
    }
}