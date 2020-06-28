using System;

namespace PinkUmbrella.Models.Auth
{
    public class AuthKey
    {
        public long Id { get; set; }

        public string PublicKey { get; set; }
        
        public AuthType Type { get; set; }
        
        public AuthKeyFormat Format { get; set; }
        
        public string Value { get; set; }

        public DateTime WhenAdded { get; set; }
    }
}