using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PinkUmbrella.Models.Auth
{
    public interface IAuthTypeHandler
    {
        AuthType Type { get; }
        
        bool HandshakeMethodSupported();
        
        HashSet<HandshakeMethod> HandshakeMethodsSupported { get; }

        Task<AuthKey> GenerateKey(HandshakeMethod method);

        Task EncryptStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth);

        Task SignStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth);

        Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, AuthKey auth);
        
        Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth);

        Task DecryptStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth);

        Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth);
    }
}