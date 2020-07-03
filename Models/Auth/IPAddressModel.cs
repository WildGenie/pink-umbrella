using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PinkUmbrella.Models.Auth
{
    public class IPAddressModel
    {
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public IPType Type { get; set; }

        [NotMapped, JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        public long PublicKeyId { get; set; }

        public PublicKey PublicKey { get; set; }

        [NotMapped]
        public IPBlockModel Block { get; set; }



        public override string ToString() => string.IsNullOrWhiteSpace(Address) ? Name : Address;
    }
}