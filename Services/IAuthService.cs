using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.Services
{
    public interface IAuthService
    {
        Task<IPAddressModel> GetOrRememberIP(IPAddress address);

        Task ForgetIPs();

        Task<AuthKeyResult> GenKey(AuthKeyOptions options, HandshakeMethod method);
        
        Task<AuthKeyResult> TryAddUserKey(PublicKey authKey, UserProfileModel user);

        Task<PublicKey> GetOrAdd(PublicKey key);

        Task<PrivateKey> GetOrAdd(PrivateKey key);

        Task Trust(IPAddress address, PublicKey key);

        Task Untrust(PublicKey key);

        Task<bool> IsTrusted(IPAddress address, PublicKey key);
        
        Task<PublicKey> GetCurrent();
        
        Task<List<PublicKey>> GetForUser(int id);
        
        Task<List<PublicKeyCredentialDescriptor>> GetCredentialsForUser(int userId);
        
        Task<List<int>> GetUserIdsByCredentialIdAsync(byte[] credentialId);
        
        Task AddCredential(UserProfileModel user, StoredCredential storedCredential);
        
        Task<StoredCredential> GetCredentialById(byte[] id);

        Task UpdateCredential(int credentialId, uint counter);
    }
}