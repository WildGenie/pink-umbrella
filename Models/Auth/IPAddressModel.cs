namespace PinkUmbrella.Models.Auth
{
    public class IPAddressModel
    {
        public long Id { get; set; }

        public IPType Type { get; set; }

        public string Address { get; set; }

        public string PublicKey { get; set; }

        public long? AuthId { get; set; }
    }
}