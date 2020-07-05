using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Auth.Permissions;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql
{
    public class AuthService : IAuthService
    {
        private readonly StringRepository _strings;
        private readonly AuthDbContext _db;
        private readonly Dictionary<AuthType, IAuthTypeHandler> _typeHandlers;

        private IQueryable<IPAddressModel> GetIPv4() => _db.IPs.Where(ip => ip.Type == IPType.IPv4);

        private IQueryable<IPAddressModel> GetIPv6() => _db.IPs.Where(ip => ip.Type == IPType.IPv6);

        public AuthService(StringRepository strings, AuthDbContext dbContext, IEnumerable<IAuthTypeHandler> typeHandlers)
        {
            _strings = strings;
            _db = dbContext;
            _typeHandlers = typeHandlers.ToDictionary(k => k.Type, v => v);
        }

        public async Task ForgetIPs()
        {
            _db.IPs.RemoveRange(_db.IPs);
            await _db.SaveChangesAsync();
        }

        public async Task<PublicKey> GetOrAdd(PublicKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var original = key.Id > 0 ? await _db.PublicKeys.FindAsync(key.Id) : !string.IsNullOrWhiteSpace(key.Value) ? await _db.PublicKeys.FirstOrDefaultAsync(k => k.Type == key.Type && k.Format == key.Format && k.Value == key.Value) : null;
            if (original == null)
            {
                key.FingerPrint = ComputeFingerPrint(key, FingerPrintType.MD5);
                key.WhenAdded = DateTime.UtcNow;
                await _db.PublicKeys.AddAsync(key);
                await _db.SaveChangesAsync();
                original = key;
            }
            
            if (string.IsNullOrWhiteSpace(original.FingerPrint))
            {
                key.FingerPrint = ComputeFingerPrint(key, FingerPrintType.MD5);
                await _db.SaveChangesAsync();
            }
            return original;
        }

        public async Task<PrivateKey> GetOrAdd(PrivateKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var original = key.Id > 0 ? await _db.PrivateKeys.FindAsync(key.Id) : !string.IsNullOrWhiteSpace(key.Value) ? await _db.PrivateKeys.FirstOrDefaultAsync(k => k.Type == key.Type && k.Format == key.Format && k.Value == key.Value) : null;
            if (original == null)
            {
                if (key.PublicKeyId == 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                key.WhenAdded = DateTime.UtcNow;
                await _db.PrivateKeys.AddAsync(key);
                await _db.SaveChangesAsync();
                original = key;
            }
            
            return original;
        }

        private string ComputeFingerPrint(PublicKey key, FingerPrintType type)
        {
            using (var hash = GetHashAlgorithm(type))
            {
                string raw = key.Value;
                switch (key.Type)
                {
                    case AuthType.OpenPGP: raw = _strings.OpenPGPKeyRegex.Match(raw).Groups["base64"].Value; break;
                    case AuthType.RSA: raw = _strings.RSAKeyRegex.Match(raw).Groups["base64"].Value; break;
                }
                raw = raw.Trim();

                byte[] bytes = null;
                switch (key.Type)
                {
                    case AuthType.OpenPGP: bytes = Encoding.ASCII.GetBytes(raw); break;
                    case AuthType.RSA: bytes = Convert.FromBase64String(raw); break;
                }
                var hashBytes = hash.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace('-', ':');
            }
        }

        private HashAlgorithm GetHashAlgorithm(FingerPrintType type)
        {
            switch (type)
            {
                case FingerPrintType.MD5: return MD5.Create();
                case FingerPrintType.SHA1: return SHA1.Create();
            }
            throw new ArgumentOutOfRangeException();
        }

        public async Task<IPAddressModel> GetOrRememberIP(IPAddress address)
        {
            var ipaddress = address.ToString();
            var ips = address.AddressFamily == AddressFamily.InterNetworkV6 ? GetIPv6() : GetIPv4();
            var ip = await ips.FirstOrDefaultAsync(ip => ip.Address == ipaddress);
            if (ip == null)
            {
                ip = new IPAddressModel()
                {
                    Type = address.AddressFamily == AddressFamily.InterNetworkV6 ? IPType.IPv6 : IPType.IPv4,
                    Address = ipaddress,
                };
                await _db.IPs.AddAsync(ip);
                await _db.SaveChangesAsync();
            }
            return ip;
        }

        public async Task<bool> IsTrusted(IPAddress address, PublicKey key)
        {
            if (address.Address == 16777343)
            {
                return true;
            }
            var ipaddress = address.ToString();
            var ips = address.AddressFamily == AddressFamily.InterNetworkV6 ? GetIPv6() : GetIPv4();
            var ip = await ips.FirstOrDefaultAsync(ip => ip.Address == ipaddress);
            var inDb = await _db.PublicKeys.FirstOrDefaultAsync(k => k.Format == key.Format && k.Type == key.Type && k.Value == key.Value);
            return inDb != null && ip != null && inDb.Id == ip.PublicKeyId;
        }

        public async Task Trust(IPAddress address, PublicKey key)
        {
            if (address.Address == 16777343)
            {
                return;
            }
            var ip = await GetOrRememberIP(address);
            var auth = await GetOrAdd(key);
            await _db.SaveChangesAsync();
            ip.PublicKeyId = auth.Id;
            await _db.SaveChangesAsync();
        }

        public async Task Untrust(PublicKey key)
        {
            var existing = await GetOrAdd(key);
            var ips = await _db.IPs.Where(ip => ip.PublicKeyId == key.Id).ToListAsync();
            if (ips.Count > 0)
            {
                foreach (var ip in ips)
                {
                    ip.PublicKeyId = 0;
                }
            }
            await _db.SaveChangesAsync();

            _db.PrivateKeys.RemoveRange(_db.PrivateKeys.Where(pk => pk.PublicKeyId == key.Id));
            await _db.SaveChangesAsync();
        }

        bool SitePermissionDefault(SitePermission perm)
        {
            switch (perm)
            {
                case SitePermission.POST_PUT: return false;
            }
            return true;
        }

        public async Task<PublicKey> GetCurrent()
        {
            var key = new PublicKey()
            {
                Type = AuthType.OpenPGP
            };

            if (_db.PublicKeys.Count() == 0)
            {
                await GenKey(new AuthKeyOptions()
                {
                    Type = key.Type
                }, HandshakeMethod.Default);
            }
            
            var first = await _db.PublicKeys.FirstAsync();
            if (string.IsNullOrWhiteSpace(first.Value))
            {
                // Regen
                await GenKey(new AuthKeyOptions()
                {
                    Type = key.Type
                }, HandshakeMethod.Default);
            }

            if (string.IsNullOrWhiteSpace(first.FingerPrint))
            {
                first.FingerPrint = ComputeFingerPrint(first, FingerPrintType.MD5);
                await _db.SaveChangesAsync();
            }

            return first;
        }

        public async Task<AuthKeyResult> GenKey(AuthKeyOptions options, HandshakeMethod method)
        {
            var ret = await _typeHandlers[options.Type].GenerateKey(options.Format, method);
            await _db.PrivateKeys.AddAsync(ret.Private);
            await _db.SaveChangesAsync();
            ret.Private.PublicKeyId = ret.Public.Id;
            await _db.PrivateKeys.AddAsync(ret.Private);
            await _db.SaveChangesAsync();
            return new AuthKeyResult()
            {
                Keys = ret,
                Error = AuthKeyError.None,
            };
        }

        public async Task<List<PublicKey>> GetForUser(int id)
        {
            var ret = await _db.UserAuthKeys.Where(k => k.UserId == id).Select(k => k.PublicKey).ToListAsync();
            var dbChanged = false;
            foreach (var k in ret)
            {
                if (string.IsNullOrWhiteSpace(k.FingerPrint))
                {
                    k.FingerPrint = ComputeFingerPrint(k, FingerPrintType.MD5);
                    dbChanged = true;
                }
            }

            if (dbChanged)
            {
                await _db.SaveChangesAsync();
            }
            return ret;
        }

        public async Task<AuthKeyResult> TryAddUserKey(PublicKey authKey, UserProfileModel user)
        {
            if (authKey == null)
            {
                throw new ArgumentNullException(nameof(authKey));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            
            if (authKey.Id == 0)
            {
                authKey.Value = authKey.Value.Trim();
                authKey.Format = AuthKeyFormat.Raw;
            }
            Regex validator = null;
            switch (authKey.Type)
            {
                case AuthType.OpenPGP: validator = _strings.OpenPGPKeyRegex; break;
                case AuthType.RSA: validator = _strings.RSAKeyRegex; break;
            }

            if (validator != null && validator.IsMatch(authKey.Value))
            {
                var newKey = await GetOrAdd(authKey);
                if (newKey != null)
                {
                    var existing = await _db.UserAuthKeys.FirstOrDefaultAsync(k => k.UserId == user.Id && k.PublicKeyId == newKey.Id);
                    if (existing == null)
                    {
                        _db.UserAuthKeys.Add(new UserAuthKeyModel()
                        {
                            PublicKeyId = newKey.Id,
                            UserId = user.Id,
                        });
                        await _db.SaveChangesAsync();
                    }
                }
                return new AuthKeyResult()
                {
                    Error = AuthKeyError.None
                };
            }
            else
            {
                return new AuthKeyResult()
                {
                    Error = AuthKeyError.InvalidFormat
                };
            }
        }

        public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsForUser(int userId)
        {
            return await _db.FIDOCredentials.Where(k => k.UserId == userId).Select(k => k.UnFurl().Descriptor).ToListAsync();
        }

        public async Task<List<int>> GetUserIdsByCredentialIdAsync(byte[] credentialId)
        {
            var credentialIdInt = BitConverter.ToInt32(credentialId);
            return await _db.FIDOCredentials.Where(c => c.Id == credentialIdInt).Select(c => c.UserId).ToListAsync();
        }

        public async Task AddCredential(UserProfileModel user, StoredCredential credential)
        {
            credential.UserId = BitConverter.GetBytes(user.Id);
            _db.FIDOCredentials.Add(new FIDOCredential(credential));
            await _db.SaveChangesAsync();
        }

        public async Task UpdateCredential(int credentialId, uint counter)
        {
            var cred = await _db.FIDOCredentials.FindAsync(credentialId);
            cred.SignatureCounter = counter;
            await _db.SaveChangesAsync();
        }

        public Task<StoredCredential> GetCredentialById(byte[] id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> LoginMethodAllowed(int userId, UserLoginMethod method, bool defaultValue)
        {
            var exists = await _db.UserLoginMethods.FirstOrDefaultAsync(ulm => ulm.Enabled && ulm.Method == method && ulm.UserId == userId);
            return exists?.Enabled ?? defaultValue;
        }

        public async Task<UpdateLoginMethodResult> UpdateLoginMethodAllowed(int userId, UserLoginMethod method, bool value, bool defaultValue)
        {
            var result = new UpdateLoginMethodResult()
            {
                Result = UpdateLoginMethodResult.ResultType.NoError
            };

            var methods = await GetOverriddenLoginMethodsForUser(userId);
            var methodsAllowed = methods.Where(m => m.Enabled).Select(m => m.Method).ToHashSet();
            var methodsRemoved = methods.Where(m => !m.Enabled).Select(m => m.Method).ToHashSet();
            var allowedMethodCountAfter = 0;
            foreach (var m in Enum.GetValues(typeof(UserLoginMethod)).Cast<UserLoginMethod>())
            {
                if (m != method)
                {
                    if (methodsAllowed.Contains(m))
                    {
                        allowedMethodCountAfter++;
                    }
                    else if (methodsRemoved.Contains(m))
                    {
                        allowedMethodCountAfter--;
                    }
                    else if (GetMethodDefault(m))
                    {
                        allowedMethodCountAfter++;
                    }
                }
                else if (value)
                {
                    allowedMethodCountAfter++;
                }
            }

            if (allowedMethodCountAfter < 1)
            {
                result.Result = UpdateLoginMethodResult.ResultType.MinimumOneAllowedLoginMethod;
                return result;
            }

            var exists = methods.FirstOrDefault(m => m.Method == method);
            if (exists != null)
            {
                if (value == defaultValue)
                {
                    _db.UserLoginMethods.Remove(exists);
                }
                else if (exists.Enabled != value)
                {
                    exists.Enabled = value;
                }
                else
                {
                    return result;
                }
            }
            else
            {
                if (value != defaultValue)
                {
                    _db.UserLoginMethods.Add(new UserLoginMethodModel()
                    {
                        Enabled = value,
                        Method = method,
                        UserId = userId,
                        WhenCreated = DateTime.UtcNow,
                    });
                }
                else
                {
                    return result;
                }
            }
            await _db.SaveChangesAsync();
            return result;
        }

        public async Task<List<UserLoginMethodModel>> GetOverriddenLoginMethodsForUser(int userId) => 
                                    await _db.UserLoginMethods.Where(ulm => ulm.UserId == userId).ToListAsync();

        public bool GetMethodDefault(UserLoginMethod method)
        {
            switch (method)
            {
                case UserLoginMethod.EmailPassword: return true;
                default: return false;
            }
        }

        public async Task<int?> GetUserByKey(PublicKey authKey)
        {
            return (await _db.UserAuthKeys.FirstOrDefaultAsync(k => k.PublicKeyId == authKey.Id))?.UserId;
        }

        public async Task<PublicKey> GetPublicKey(string key, AuthType type)
        {
            return await _db.PublicKeys.FirstOrDefaultAsync(k => k.Type == type && k.Value == key);
        }

        public IAuthTypeHandler GetHandler(AuthType type) => _typeHandlers[type];

        public Task<PrivateKey> GetPrivateKey(long publicKeyId, AuthType type)
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> GenChallenge(PublicKey pubkey, DateTime? expires)
        {
            expires = expires ?? DateTime.UtcNow.AddDays(1);
            var answer = new byte[32];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(answer);

            byte[] challengeBytes;
            using (var answerStream = new MemoryStream(answer))
            {
                using var challengeStream = new MemoryStream();
                await GetHandler(pubkey.Type).EncryptStreamAsync(answerStream, challengeStream, pubkey);
                challengeStream.Position = 0;
                challengeBytes = challengeStream.ToArray();
            }

            var challenge = new KeyChallengeModel()
            {
                Challenge = challengeBytes,
                Expires = expires.Value,
                KeyId = pubkey.Id
            };

            await _db.KeyChallenges.AddAsync(challenge);
            await _db.SaveChangesAsync();
            return challenge.Challenge;
        }

        public async Task<byte[]> GetChallenge(PublicKey pubkey)
        {
            var now = DateTime.UtcNow;
            return (await _db.KeyChallenges.FirstOrDefaultAsync(kc => kc.KeyId == pubkey.Id && kc.Expires > now))?.Challenge;
        }

        public AuthType ResolveType(string key)
        {
            if (_strings.OpenPGPKeyRegex.IsMatch(key))
            {
                return AuthType.OpenPGP;
            }
            else if (_strings.RSAKeyRegex.IsMatch(key))
            {
                return AuthType.RSA;
            }
            else
            {
                return AuthType.None;
            }
        }
    }
}