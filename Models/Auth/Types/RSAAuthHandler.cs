using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Models.Auth.Types
{
    public class RSAAuthHandler : IAuthTypeHandler
    {
        private readonly RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();
        private readonly StringRepository _strings;

        public RSAAuthHandler(StringRepository strings)
        {
            _strings = strings;
        }
        
        public AuthType Type { get; } = AuthType.RSA;

        public HashSet<HandshakeMethod> HandshakeMethodsSupported => throw new System.NotImplementedException();

        public Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }

        public Task DecryptStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey)
        {
            throw new NotImplementedException();
        }

        public Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PublicKey publicKey, PrivateKey privateKey)
        {
            return Task.CompletedTask;
        }

        public Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }

        public async Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey auth)
        {
            var original = ToPEM(auth.Value);
            var _rsa = new RSACryptoServiceProvider();
            int numRead = 0;
            _rsa.ImportRSAPublicKey(Convert.FromBase64String(original), out numRead);
            var buffer = new byte[_rsa.KeySize / 8 - 11];
            var totalWritten = 0;
            while (true)
            {
                numRead = await inputStream.ReadAsync(buffer);
                if (numRead > 0)
                {
                    var todo = new byte[numRead];
                    Array.Copy(buffer, todo, numRead);
                    var outBuffer = _rsa.Encrypt(todo, RSAEncryptionPadding.OaepSHA1);
                    totalWritten += outBuffer.Length;
                    await outputStream.WriteAsync(outBuffer);
                }
                else
                {
                    await outputStream.FlushAsync();
                    break;
                }
            }
        }

        private byte[] ToX509(string value) => Encoding.ASCII.GetBytes(ToPEM(value));

        private string ToPEM(string value)
        {
            var ret = new StringBuilder();
            ret.Append("-----BEGIN RSA PUBLIC KEY-----\n");
            ret.Append(_strings.RSAKeyRegex.Match(value).Groups["base64"].Value);
            ret.Append("\n-----END RSA PUBLIC KEY-----");
            return ret.ToString();
        }

        public Task<KeyPair> GenerateKey(AuthKeyFormat format, HandshakeMethod method)
        {
            var rsaKeyInfo = _rsa.ExportParameters(true);
            return Task.FromResult(new KeyPair()
            {
                Public = new PublicKey()
                {
                    Value = Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo()) 
                },
                Private = new PrivateKey()
                {
                    Value = Convert.ToBase64String(_rsa.ExportRSAPrivateKey()) 
                },
            });
        }

        public bool HandshakeMethodSupported(HandshakeMethod method)
        {
            return false;
        }

        public Task SignStreamAsync(Stream inputStream, Stream outputStream, PrivateKey auth)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }
    }
}