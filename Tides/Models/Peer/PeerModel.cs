using System.ComponentModel;
using System.Text.Json.Serialization;
using Tides.Models.Auth;
using Tides.Util;

namespace Tides.Models.Peer
{
    [IsDocumented, DisplayName("Peer"), Description("Basic information about another running instance of this site")]
    public class PeerModel
    {
        [JsonPropertyName("address"), Description("IPv4 or IPv6 address")]
        public IPAddressModel Address { get; set; }

        [JsonPropertyName("port"), DisplayName("Port"), Description("Port on which the socket streams on")]
        public int AddressPort { get; set; }

        [JsonPropertyName("displayName"), DisplayName("Display Name"), Description("IPv4 or IPv6 address")]
        public string DisplayName { get; set; }

        [JsonPropertyName("publicKey"), DisplayName("Public Key"), Description("The public key used for verification")]
        public PublicKey PublicKey { get; set; }



        public override string ToString() => $"Peer ({DisplayName}) at {Address}:{AddressPort}";
    }
}