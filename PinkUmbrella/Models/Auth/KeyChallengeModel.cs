using System;

namespace PinkUmbrella.Models.Auth
{
    public class KeyChallengeModel
    {
        public long Id { get; set; }
        
        public long KeyId { get; set; }

        public byte[] Challenge { get; set; }

        public DateTime Expires { get; set; }
    }
}