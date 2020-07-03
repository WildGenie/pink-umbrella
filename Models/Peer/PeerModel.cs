using System.Text.Json.Serialization;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.Models.Peer
{
    public class PeerModel
    {
        [JsonPropertyName("address")]
        public IPAddressModel Address { get; set; }

        [JsonPropertyName("port")]
        public int AddressPort { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("publicKey")]
        public PublicKey PublicKey { get; set; }
    }
}