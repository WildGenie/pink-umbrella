using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Poncho.Models.Auth
{
    public interface IAuthTypeHandler
    {
        AuthType Type { get; }
        AuthKeyFormat Format { get; }
        
        bool HandshakeMethodSupported(HandshakeMethod method);
        
        HashSet<HandshakeMethod> HandshakeMethodsSupported { get; }

        Task<KeyPair> GenerateKey(HandshakeMethod method);

        Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey auth);

        Task SignStreamAsync(Stream inputStream, Stream outputStream, PrivateKey auth, Func<string> passwordFinder);

        Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder);
        
        Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder);

        Task DecryptStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, Func<string> passwordFinder);

        Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey);
        
        string ComputeFingerPrint(PublicKey key, HashAlgorithm alg);
    }
}