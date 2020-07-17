using System;
using System.Collections.Generic;
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
using Poncho.Models.Auth;
using Poncho.Util;

namespace PinkUmbrella.Services.Sql
{
    public class AuthService : IAuthService
    {
        private readonly StringRepository _strings;
        private readonly AuthDbContext _db;
        private readonly Dictionary<AuthType, Dictionary<AuthKeyFormat, IAuthTypeHandler>> _typeHandlers;

        private IQueryable<SavedIPAddressModel> GetIPv4() => _db.IPs.Where(ip => ip.Type == IPType.IPv4);

        private IQueryable<SavedIPAddressModel> GetIPv6() => _db.IPs.Where(ip => ip.Type == IPType.IPv6);

        public AuthService(StringRepository strings, AuthDbContext dbContext, IEnumerable<IAuthTypeHandler> typeHandlers)
        {
            _strings = strings;
            _db = dbContext;
            _typeHandlers = typeHandlers.GroupBy(g => g.Type).ToDictionary(k => k.Key, v => v.ToDictionary(k => k.Format, v => v));
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

            var hasValue = false;
            if (!string.IsNullOrWhiteSpace(key.Value))
            {
                key.Value = key.Value.Replace("\r", "");
                hasValue = true;

                var resolvedFormat = AuthKeyFormatResolver.Resolve(key.Value);
                if (!resolvedFormat.HasValue)
                {
                    throw new ArgumentException($"Input key does not resolve to a valid format");
                }
                else if (key.Format == AuthKeyFormat.Error)
                {
                    key.Format = resolvedFormat.Value;
                }
                else if (key.Format != resolvedFormat)
                {
                    throw new ArgumentException($"Input key format ({key.Format}) does not match resolved format ({resolvedFormat})");
                }
            }

            var original = key.Id > 0 ? await _db.PublicKeys.FindAsync(key.Id) : hasValue ? await _db.PublicKeys.FirstOrDefaultAsync(k => k.Type == key.Type && k.Format == key.Format && k.Value == key.Value) : null;
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
                if (!key.PublicKeyId.HasValue || key.PublicKeyId.Value == 0)
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
                string raw = key.Value ?? throw new ArgumentNullException("value");
                switch (key.Type)
                {
                    case AuthType.OpenPGP: raw = RegexHelper.OpenPGPKeyRegex.Match(raw).Groups["base64"].Value; break;
                    case AuthType.RSA: raw = RegexHelper.RSAKeyRegex.Match(raw).Groups["base64"].Value; break;
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

        public async Task<SavedIPAddressModel> GetOrRememberIP(IPAddress address)
        {
            var ipaddress = address.ToString();
            var ips = address.AddressFamily == AddressFamily.InterNetworkV6 ? GetIPv6() : GetIPv4();
            var ip = await ips.FirstOrDefaultAsync(ip => ip.Address == ipaddress);
            if (ip == null)
            {
                ip = new SavedIPAddressModel()
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
            if (address.Address == LOCALHOST_ADDRESS)
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
            if (address.Address == LOCALHOST_ADDRESS)
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
                    ip.PublicKeyId = null;
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
            var ret = await _typeHandlers[options.Type][options.Format].GenerateKey(method);
            if (string.IsNullOrWhiteSpace(ret.Public.FingerPrint))
            {
                ret.Public.FingerPrint = ComputeFingerPrint(ret.Public, FingerPrintType.MD5);
            }
            // if (string.IsNullOrWhiteSpace(ret.Private.FingerPrint))
            // {
            //     ret.Private.FingerPrint = ComputeFingerPrint(ret.Private, FingerPrintType.MD5);
            // }

            await _db.PublicKeys.AddAsync(ret.Public);
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

        public async Task<AuthKeyResult> GenApiKey(PublicKey clientKey, HandshakeMethod method)
        {
            if (clientKey == null || clientKey.Id <= 0 || string.IsNullOrWhiteSpace(clientKey.Value))
            {
                throw new ArgumentNullException(nameof(clientKey));
            }

            var key = await GenKey(new AuthKeyOptions() { Format = clientKey.Format, Type = clientKey.Type }, method);
            if (key.Error == AuthKeyError.None)
            {
                await _db.ApiAuthKeys.AddAsync(new ApiAuthKeyModel() { ServerPrivateKeyId = key.Keys.Private.Id, ServerPublicKeyId = key.Keys.Public.Id, ClientPublicKeyId = clientKey.Id });
                await _db.SaveChangesAsync();
            }
            return key;
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
                case AuthType.OpenPGP: validator = RegexHelper.OpenPGPKeyRegex; break;
                case AuthType.RSA: validator = RegexHelper.RSAKeyRegex; break;
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

        public IAuthTypeHandler GetHandler(AuthType type, AuthKeyFormat format) => _typeHandlers[type][format];

        public Task<PrivateKey> GetPrivateKey(long publicKeyId, AuthType type)
        {
            return _db.PrivateKeys.FirstOrDefaultAsync(k => k.PublicKeyId == publicKeyId && k.Type == type);
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
                await GetHandler(pubkey.Type, pubkey.Format).EncryptStreamAsync(answerStream, challengeStream, pubkey);
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
            if (RegexHelper.OpenPGPKeyRegex.IsMatch(key))
            {
                return AuthType.OpenPGP;
            }
            else if (RegexHelper.RSAKeyRegex.IsMatch(key))
            {
                return AuthType.RSA;
            }
            else
            {
                return AuthType.None;
            }
        }

        public List<string> ToBiometric(byte[] vs) => vs.Select(ToBiometric).ToList();


        private static readonly string[] pgpEvenWords = { "aardvark", "absurd", "accrue", "acme", "adrift", "adult", "afflict", "ahead", "aimless", "Algol", "allow", "alone", "ammo", "ancient", "apple", "artist", "assume", "Athens", "atlas", "Aztec", "baboon", "backfield", "backward", "banjo", "beaming", "bedlamp", "beehive", "beeswax", "befriend", "Belfast", "berserk", "billiard", "bison", "blackjack", "blockade", "blowtorch", "bluebird", "bombast", "bookshelf", "brackish", "breadline", "breakup", "brickyard", "briefcase", "Burbank", "button", "buzzard", "cement", "chairlift", "chatter", "checkup", "chisel", "choking", "chopper", "Christmas", "clamshell", "classic", "classroom", "cleanup", "clockwork", "cobra", "commence", "concert", "cowbell", "crackdown", "cranky", "crowfoot", "crucial", "crumpled", "crusade", "cubic", "dashboard", "deadbolt", "deckhand", "dogsled", "dragnet", "drainage", "dreadful", "drifter", "dropper", "drumbeat", "drunken", "Dupont", "dwelling", "eating", "edict", "egghead", "eightball", "endorse", "endow", "enlist", "erase", "escape", "exceed", "eyeglass", "eyetooth", "facial", "fallout", "flagpole", "flatfoot", "flytrap", "fracture", "framework", "freedom", "frighten", "gazelle", "Geiger", "glitter", "glucose", "goggles", "goldfish", "gremlin", "guidance", "hamlet", "highchair", "hockey", "indoors", "indulge", "inverse", "involve", "island", "jawbone", "keyboard", "kickoff", "kiwi", "klaxon", "locale", "lockup", "merit", "minnow", "miser", "Mohawk", "mural", "music", "necklace", "Neptune", "newborn", "nightbird", "Oakland", "obtuse", "offload", "optic", "orca", "payday", "peachy", "pheasant", "physique", "playhouse", "Pluto", "preclude", "prefer", "preshrunk", "printer", "prowler", "pupil", "puppy", "python", "quadrant", "quiver", "quota", "ragtime", "ratchet", "rebirth", "reform", "regain", "reindeer", "rematch", "repay", "retouch", "revenge", "reward", "rhythm", "ribcage", "ringbolt", "robust", "rocker", "ruffled", "sailboat", "sawdust", "scallion", "scenic", "scorecard", "Scotland", "seabird", "select", "sentence", "shadow", "shamrock", "showgirl", "skullcap", "skydive", "slingshot", "slowdown", "snapline", "snapshot", "snowcap", "snowslide", "solo", "southward", "soybean", "spaniel", "spearhead", "spellbind", "spheroid", "spigot", "spindle", "spyglass", "stagehand", "stagnate", "stairway", "standard", "stapler", "steamship", "sterling", "stockman", "stopwatch", "stormy", "sugar", "surmount", "suspense", "sweatband", "swelter", "tactics", "talon", "tapeworm", "tempest", "tiger", "tissue", "tonic", "topmost", "tracker", "transit", "trauma", "treadmill", "Trojan", "trouble", "tumor", "tunnel", "tycoon", "uncut", "unearth", "unwind", "uproot", "upset", "upshot", "vapor", "village", "virus", "Vulcan", "waffle", "wallet", "watchword", "wayside", "willow", "woodlark", "Zulu" };
        private static readonly string[] pgpOddWords = { "adroitness", "adviser", "aftermath", "aggregate", "alkali", "almighty", "amulet", "amusement", "antenna", "applicant", "Apollo", "armistice", "article", "asteroid", "Atlantic", "atmosphere", "autopsy", "Babylon", "backwater", "barbecue", "belowground", "bifocals", "bodyguard", "bookseller", "borderline", "bottomless", "Bradbury", "bravado", "Brazilian", "breakaway", "Burlington", "businessman", "butterfat", "Camelot", "candidate", "cannonball", "Capricorn", "caravan", "caretaker", "celebrate", "cellulose", "certify", "chambermaid", "Cherokee", "Chicago", "clergyman", "coherence", "combustion", "commando", "company", "component", "concurrent", "confidence", "conformist", "congregate", "consensus", "consulting", "corporate", "corrosion", "councilman", "crossover", "crucifix", "cumbersome", "customer", "Dakota", "decadence", "December", "decimal", "designing", "detector", "detergent", "determine", "dictator", "dinosaur", "direction", "disable", "disbelief", "disruptive", "distortion", "document", "embezzle", "enchanting", "enrollment", "enterprise", "equation", "equipment", "escapade", "Eskimo", "everyday", "examine", "existence", "exodus", "fascinate", "filament", "finicky", "forever", "fortitude", "frequency", "gadgetry", "Galveston", "getaway", "glossary", "gossamer", "graduate", "gravity", "guitarist", "hamburger", "Hamilton", "handiwork", "hazardous", "headwaters", "hemisphere", "hesitate", "hideaway", "holiness", "hurricane", "hydraulic", "impartial", "impetus", "inception", "indigo", "inertia", "infancy", "inferno", "informant", "insincere", "insurgent", "integrate", "intention", "inventive", "Istanbul", "Jamaica", "Jupiter", "leprosy", "letterhead", "liberty", "maritime", "matchmaker", "maverick", "Medusa", "megaton", "microscope", "microwave", "midsummer", "millionaire", "miracle", "misnomer", "molasses", "molecule", "Montana", "monument", "mosquito", "narrative", "nebula", "newsletter", "Norwegian", "October", "Ohio", "onlooker", "opulent", "Orlando", "outfielder", "Pacific", "pandemic", "Pandora", "paperweight", "paragon", "paragraph", "paramount", "passenger", "pedigree", "Pegasus", "penetrate", "perceptive", "performance", "pharmacy", "phonetic", "photograph", "pioneer", "pocketful", "politeness", "positive", "potato", "processor", "provincial", "proximate", "puberty", "publisher", "pyramid", "quantity", "racketeer", "rebellion", "recipe", "recover", "repellent", "replica", "reproduce", "resistor", "responsive", "retraction", "retrieval", "retrospect", "revenue", "revival", "revolver", "sandalwood", "sardonic", "Saturday", "savagery", "scavenger", "sensation", "sociable", "souvenir", "specialist", "speculate", "stethoscope", "stupendous", "supportive", "surrender", "suspicious", "sympathy", "tambourine", "telephone", "therapist", "tobacco", "tolerance", "tomorrow", "torpedo", "tradition", "travesty", "trombonist", "truncated", "typewriter", "ultimate", "undaunted", "underfoot", "unicorn", "unify", "universe", "unravel", "upcoming", "vacancy", "vagabond", "vertigo", "Virginia", "visitor", "vocalist", "voyager", "warranty", "Waterloo", "whimsical", "Wichita", "Wilmington", "Wyoming", "yesteryear", "Yucatan" };
        private static readonly long LOCALHOST_ADDRESS = 16777343;

        private string ToBiometric(byte b, int i)
        {
            // https://en.wikipedia.org/wiki/PGP_word_list
            if (i % 2 == 0)
            {
                return pgpEvenWords[b];
            }
            else
            {
                return pgpOddWords[b];
            }
        }

        public async Task<List<RecoveryKeyModel>> GetRecoveryKeys(int userId) => await _db.RecoveryKeys.Where(key => key.UserId == userId).ToListAsync();

        public async Task<List<RecoveryKeyModel>> CreateRecoveryKeys(int userId, string label, int length, int count)
        {
            if (length <= 0)
            {
                throw new ArgumentException("length is less than or equal to 0", nameof(length));
            }
            else if (length < 6)
            {
                throw new ArgumentException("length too small, should be 6 or more", nameof(length));
            }
            else if (length > 99)
            {
                throw new ArgumentException("length too large, should be 99 or smaller", nameof(length));
            }
            else if (length == 1)
            {
                label += " ({0})";
            }

            var ret = new List<RecoveryKeyModel>();
            for (int i = 0; i < count; i++)
            {
                var code = new StringBuilder();
                var buf = new byte[1];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    for (int c = 0; c < length; c++)
                    {
                        while (true)
                        {
                            rng.GetNonZeroBytes(buf);
                            var ch = (char)buf[0];
                            if (_strings.RecoveryCodeChars.Contains(ch))
                            {
                                code.Append(ch);
                                break;
                            }
                        }
                    }
                }
                ret.Add(new RecoveryKeyModel()
                {
                    UserId = userId,
                    Code = code.ToString(),
                    WhenCreated = DateTime.UtcNow,
                    Label = length == 1 ? label : string.Format(label, i + 1),
                });
            }
            await _db.RecoveryKeys.AddRangeAsync(ret);
            await _db.SaveChangesAsync();
            return ret;
        }

        public async Task SaveAsync() => await _db.SaveChangesAsync();

        public async Task DeleteRecoveryKey(RecoveryKeyModel key)
        {
            _db.RecoveryKeys.Remove(key);
            await _db.SaveChangesAsync();
        }

        public async Task<List<ApiAuthKeyModel>> GetApiKeys()
        {
            var ret = await _db.ApiAuthKeys.ToListAsync();
            foreach (var apiKey in ret)
            {
                await BindReferences(apiKey);
            }
            return ret;
        }

        private async Task BindReferences(ApiAuthKeyModel apiKey)
        {
            if (apiKey == null)
            {
                return;
            }

            if (apiKey.ClientPublicKey == null)
            {
                apiKey.ClientPublicKey = await _db.PublicKeys.FindAsync(apiKey.ClientPublicKeyId);
            }
            if (apiKey.ServerPublicKey == null)
            {
                apiKey.ServerPublicKey = await _db.PublicKeys.FindAsync(apiKey.ServerPublicKeyId);
            }
            if (apiKey.ServerPrivateKey == null)
            {
                apiKey.ServerPrivateKey = await _db.PrivateKeys.FindAsync(apiKey.ServerPrivateKeyId);
            }
        }

        public async Task<ApiAuthKeyModel> GetApiKey(PublicKey key)
        {
            var ret = await _db.ApiAuthKeys.FirstOrDefaultAsync(k => k.ClientPublicKeyId == key.Id);
            await BindReferences(ret);
            return ret;
        }

        public async Task<KeyPair> GetKeyPair(PublicKey key)
        {
            if (key != null)
            {
                var privateKey = await GetPrivateKey(key.Id, key.Type);
                return new KeyPair { Private = privateKey, Public = key };
            }
            return null;
        }
    }
}