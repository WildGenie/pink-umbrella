using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Auth;
using Poncho.Models.Auth;

namespace PinkUmbrella.Services
{
    public interface IAuthService
    {
        Task<SavedIPAddressModel> GetOrRememberIP(IPAddress address);

        Task ForgetIPs();

        IAuthTypeHandler GetHandler(AuthType type, AuthKeyFormat format);

        Task<AuthKeyResult> GenKey(AuthKeyOptions options, HandshakeMethod method);

        Task<AuthKeyResult> GenApiKey(PublicKey clientKey, HandshakeMethod method);
        
        Task<AuthKeyResult> TryAddUserKey(PublicKey authKey, UserProfileModel user);
        
        Task<int?> GetUserByKey(PublicKey authKey);

        Task<PublicKey> GetOrAdd(PublicKey key);

        // Task<PrivateKey> GetOrAdd(PrivateKey key);

        Task<KeyPair> GetKeyPair(PublicKey key);

        Task<List<ApiAuthKeyModel>> GetApiKeys();

        Task<ApiAuthKeyModel> GetApiKey(PublicKey key);

        Task Trust(IPAddress address, PublicKey key);

        Task Untrust(PublicKey key);

        Task<bool> IsTrusted(IPAddress address, PublicKey key);

        Task<PublicKey> GetCurrent();
        
        Task<PrivateKey> GetCurrentPrivateKey();
        
        Task<List<PublicKey>> GetForUser(int id);
        
        Task<List<PublicKeyCredentialDescriptor>> GetCredentialsForUser(int userId);
        
        Task<List<int>> GetUserIdsByCredentialIdAsync(byte[] credentialId);
        
        Task AddCredential(UserProfileModel user, StoredCredential storedCredential);
        
        Task<StoredCredential> GetCredentialById(byte[] id);

        Task UpdateCredential(int credentialId, uint counter);

        Task<bool> LoginMethodAllowed(int userId, UserLoginMethod method, bool defaultValue);

        Task<UpdateLoginMethodResult> UpdateLoginMethodAllowed(int userId, UserLoginMethod method, bool value, bool defaultValue);

        Task<List<UserLoginMethodModel>> GetOverriddenLoginMethodsForUser(int userId);

        bool GetMethodDefault(UserLoginMethod method);
        
        Task<PublicKey> GetPublicKey(string key, AuthType type);
        
        // Task<PrivateKey> GetPrivateKey(long publicKeyId, AuthType type);
        
        Task<byte[]> GenChallenge(PublicKey pubkey, DateTime? expires);
        
        Task<byte[]> GetChallenge(PublicKey pubkey);
        
        AuthType ResolveType(string key);
        
        List<string> ToBiometric(byte[] vs);
        
        Task<List<RecoveryKeyModel>> GetRecoveryKeys(int userId);
        
        Task<List<RecoveryKeyModel>> CreateRecoveryKeys(int userId, string label, int length, int count);
        
        Task DeleteRecoveryKey(RecoveryKeyModel key);
        
        Task SaveAsync();
        
        Task DeleteApiKey(long clientPublicKeyId);

        string CleanupKeyString(string key);
    }
}