using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PinkUmbrella.Models.Auth
{
    public interface IAuthTypeHandler
    {
        AuthType Type { get; }
        
        bool HandshakeMethodSupported(HandshakeMethod method);
        
        HashSet<HandshakeMethod> HandshakeMethodsSupported { get; }

        Task<KeyPair> GenerateKey(AuthKeyFormat format, HandshakeMethod method);

        Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey auth);

        Task SignStreamAsync(Stream inputStream, Stream outputStream, PrivateKey auth);

        Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey);
        
        Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey);

        Task DecryptStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey);

        Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey);
    }
}