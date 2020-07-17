using Poncho.Models.Auth;

namespace PinkUmbrella.Models.Auth
{
    public class UserAuthKeyModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long? PublicKeyId { get; set; }
        public PublicKey PublicKey { get; set; }
    }
}