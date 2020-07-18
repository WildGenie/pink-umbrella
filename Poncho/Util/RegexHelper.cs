using System.Text.RegularExpressions;

namespace Poncho.Util
{
    public static class RegexHelper
    {
        public static readonly Regex OpenPGPKeyRegex = new Regex(@"(?:-+BEGIN PGP PUBLIC KEY BLOCK-+\n([a-z0-9.#:-]+( |\b))+)(?<base64>[a-z0-9\/+ \n=]+)(?:-+END PGP PUBLIC KEY BLOCK.*)", RegexOptions.IgnoreCase);
        public static readonly Regex RSAKeyPEMRegex = new Regex(@"(?:-+BEGIN (RSA)? (PRIVATE|PUBLIC) KEY-+\n[a-z0-9._!@#$%^&*()+=\[\]{}:; -]+\s*)(?<base64>[a-z0-9/+\n=]+)(?:-+END (RSA)? (PRIVATE|PUBLIC) KEY.*)", RegexOptions.IgnoreCase);
        public static readonly Regex RSAKeyRegex = new Regex(@"(?:ssh-rsa|ssh-ed25519|ecdsa-sha2-nistp256|ecdsa-sha2-nistp384|ecdsa-sha2-nistp521)(?:\s+)(?<base64>[a-z0-9/+]+)(?:.*)", RegexOptions.IgnoreCase);
        public static readonly Regex Base64Regex = new Regex(@"(?<base64>[a-z0-9/+]+)(?:.*)", RegexOptions.IgnoreCase);
    }
}