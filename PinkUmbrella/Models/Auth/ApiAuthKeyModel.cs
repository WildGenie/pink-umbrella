using Poncho.Models.Auth;

namespace PinkUmbrella.Models.Auth
{
    public class ApiAuthKeyModel
    {
        public int Id { get; set; }
        public long ClientPublicKeyId { get; set; }
        public long ServerPublicKeyId { get; set; }
        public long ServerPrivateKeyId { get; set; }
        public PublicKey ClientPublicKey { get; set; }
        public PublicKey ServerPublicKey { get; set; }
        // public PrivateKey ServerPrivateKey { get; set; }
    }
}