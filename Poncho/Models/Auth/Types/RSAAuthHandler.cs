using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.OpenSsl;
using Poncho.Util;

namespace Poncho.Models.Auth.Types
{
    public abstract class RSAAuthHandler : IAuthTypeHandler
    {
        public RSAAuthHandler() { }
        
        public AuthType Type { get; } = AuthType.RSA;

        public HashSet<HandshakeMethod> HandshakeMethodsSupported => throw new System.NotImplementedException();

        public abstract AuthKeyFormat Format { get; }

        public class PasswordFinder : IPasswordFinder
        {
            public Func<string> Get { get; set; }

            public char[] GetPassword() => Get().ToCharArray();
        }

        private byte[] ToX509(string value) => Encoding.ASCII.GetBytes(ToPEM(value));

        private string ToPEM(string value)
        {
            var ret = new StringBuilder();
            ret.Append("-----BEGIN RSA PUBLIC KEY-----\n");
            if (RegexHelper.RSAKeyRegex.Match(value).Groups.TryGetValue("base64", out var base64))
            {
                ret.Append(base64.Value);
            }
            else
            {
                ret.Append(value);
            }
            ret.Append("\n-----END RSA PUBLIC KEY-----\n");
            return ret.ToString();
        }

        public bool HandshakeMethodSupported(HandshakeMethod method) => method == HandshakeMethod.Default;

        public abstract Task<KeyPair> GenerateKey(HandshakeMethod method);
        public abstract Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey auth);
        public abstract Task SignStreamAsync(Stream inputStream, Stream outputStream, PrivateKey auth, Func<string> passwordFinder);
        public abstract Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder);
        public abstract Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder);
        public abstract Task DecryptStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, Func<string> passwordFinder);
        public abstract Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey);
        public abstract string ComputeFingerPrint(PublicKey key, HashAlgorithm alg);
    }
}