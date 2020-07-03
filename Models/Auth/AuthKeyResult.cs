namespace PinkUmbrella.Models.Auth
{
    public class AuthKeyResult
    {
        public AuthKeyError Error { get; set; }

        public KeyPair Keys { get; set; }
    }
}