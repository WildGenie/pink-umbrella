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

        public Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            throw new System.NotImplementedException();
        }

        public Task DecryptStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            throw new System.NotImplementedException();
        }

        public Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, AuthKey auth)
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

        public Task EncryptStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            throw new System.NotImplementedException();
        }

        public Task<AuthKey> GenerateKey(HandshakeMethod method)
        {
            var rsaKeyInfo = _rsa.ExportParameters(true);
            return Task.FromResult(new AuthKey()
            {
                Format = AuthKeyFormat.Raw,
                PublicKey = Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo()) 
            });
        }

        public bool HandshakeMethodSupported()
        {
            throw new System.NotImplementedException();
        }

        public Task SignStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            throw new System.NotImplementedException();
        }
    }
}