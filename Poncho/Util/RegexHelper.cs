using System.Text.RegularExpressions;

namespace Poncho.Util
{
    public static class RegexHelper
    {
        public static readonly Regex OpenPGPKeyRegex = new Regex(@"(?:-+BEGIN PGP PUBLIC KEY BLOCK-+\n[a-zA-Z0-9._!@#$%^&*()+=\[\]{}:; -]+\n+)(?<base64>[a-zA-Z0-9/+\n=]+)(?:-+END PGP PUBLIC KEY BLOCK.*)");
        public static readonly Regex RSAKeyRegex = new Regex(@"(?:ssh-rsa|ssh-ed25519|ecdsa-sha2-nistp256|ecdsa-sha2-nistp384|ecdsa-sha2-nistp521)(?:\s+)(?<base64>[a-zA-Z0-9/+]+)(?:.*)");
        public static readonly Regex Base64Regex = new Regex(@"(?<base64>[a-zA-Z0-9/+]+)(?:.*)");
    }
}