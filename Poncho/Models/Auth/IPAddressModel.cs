using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json.Serialization;

namespace Poncho.Models.Auth
{
    public class IPAddressModel
    {
        [JsonPropertyName("type")]
        public IPType Type { get; set; }

        [NotMapped, JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [DefaultValue(null)]
        public long? PublicKeyId { get; set; }

        public PublicKey PublicKey { get; set; }



        public override string ToString() => string.IsNullOrWhiteSpace(Address) ? Name : Address;

        public IPAddress ToIp() => IPAddress.Parse(Address ?? Name);
    }
}