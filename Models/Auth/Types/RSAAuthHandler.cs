using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PinkUmbrella.Models.Auth.Types
{
    public class RSAAuthHandler : IAuthTypeHandler
    {
        private readonly RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();
        
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
        // [JsonPropertyName("peerPublicKey")]
        // public string PeerPublicKey { get; set; }

        // [JsonPropertyName("publicAccessKey")]
        // public string PublicAccessKey => Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo());

        // [JsonPropertyName("rsaXml"), IsSecret]
        // public string RsaXML { get; set; }

        // [JsonIgnore]
        // public RSAParameters Rsa {
        //     get
        //     {
        //         _rsa.FromXmlString(RsaXML);
        //         return _rsa.ExportParameters(true);
        //     }
        //     set
        //     {
        //         _rsa.ImportParameters(value);
        //         RsaXML = _rsa.ToXmlString(true);
        //     }
        // }
        
        // [JsonIgnore]
        // public string PrivateAccessKey => Convert.ToBase64String(_rsa.ExportPkcs8PrivateKey());
        }

        public Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }

        public Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey auth)
        {
            throw new NotImplementedException();
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