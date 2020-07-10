using PinkUmbrella.Util;

namespace PinkUmbrella.Models.Auth
{
    public enum UserLoginMethod
    {
        [Name("Email and password")]
        EmailPassword = 0,

        [Name("Public and private key")]
        PublicPrivateKey = 1,

        [Name("FIDO (Fast Identity Online)")]
        FIDO = 2,

        [Name("OpenAuth")]
        OAuth = 3,

        [Name("Recovery key")]
        RecoveryKey = 4,
    }
}