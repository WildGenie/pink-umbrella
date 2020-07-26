using System.ComponentModel;
using System.Text.Json.Serialization;
using Tides.Models.Auth;
using Tides.Models.Peer;
using Tides.Util;
using static Tides.Actors.Common;

namespace Tides.Actors
{
    [IsDocumented, DisplayName("Peer"), Description("Basic information about another running instance of this site")]
    public class Peer: ActorObject
    {
        public Peer(string type = null) : base(type ?? nameof(Peer)) { }

        public PeerStatsModel stats { get; set; }

        [JsonPropertyName("address"), Description("IPv4 or IPv6 address")]
        public IPAddressModel Address { get; set; }

        [JsonPropertyName("port"), DisplayName("Port"), Description("Port on which the socket streams on")]
        public int AddressPort { get; set; }

        [JsonPropertyName("publicKey"), DisplayName("Public Key"), Description("The public key used for verification")]
        public PublicKey PublicKey { get; set; }


        public override string ToString() => $"Peer ({name}) at {Address}:{AddressPort}";
    }
}