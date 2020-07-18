using System;
using System.IO;
using System.Threading.Tasks;
using Poncho.Models.Auth;

namespace Poncho.Models.Crypto
{
    public class BaseKeyManager
    {
        public readonly string PUBLIC_KEY_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ssh/id_pink.pub";
        
        public readonly string PRIVATE_KEY_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ssh/id_pink";

        public string PUBLIC_KEY => ReadKey(PUBLIC_KEY_PATH);

        public string PRIVATE_KEY=> ReadKey(PRIVATE_KEY_PATH);
        
        public AuthType PUBLIC_KEY_TYPE => ReadKeyType(PUBLIC_KEY_PATH);
        
        public AuthType PRIVATE_KEY_TYPE => ReadKeyType(PRIVATE_KEY_PATH);

        private string ReadKey(string path)
        {
            var key = File.ReadAllText(path);
            var sep = key.IndexOf(' ');
            return key.Substring(sep + 1);
        }

        private AuthType ReadKeyType(string path)
        {
            var buffer = new byte[32];
            using var fileReader = File.OpenRead(path);
            fileReader.Read(buffer);
            var str = System.Text.Encoding.UTF8.GetString(buffer);
            var sep = str.IndexOf(' ');
            return (AuthType) Enum.Parse(typeof(AuthType), str.Substring(0, sep));
        }

        public KeyPair MyKeys => new KeyPair { Public = publicKey, Private = privateKey };

        public PublicKey publicKey => new PublicKey { Value = PUBLIC_KEY, Type = PUBLIC_KEY_TYPE, Format = AuthKeyFormatResolver.Resolve(PUBLIC_KEY).Value };
        
        public PrivateKey privateKey => new PrivateKey { Value = PRIVATE_KEY, Type = PRIVATE_KEY_TYPE, Format = AuthKeyFormatResolver.Resolve(PRIVATE_KEY).Value };

        public bool Exists => File.Exists(PUBLIC_KEY_PATH);

        public void Delete()
        {
            File.Delete(PUBLIC_KEY_PATH);
            File.Delete(PRIVATE_KEY_PATH);
        }

        public async Task Generate(IAuthTypeHandler handler)
        {
            var key = await handler.GenerateKey(HandshakeMethod.Default);
            key.Public.Value = CleanupKeyString(key.Public.Value);
            key.Private.Value = CleanupKeyString(key.Private.Value);
            await File.WriteAllTextAsync(PUBLIC_KEY_PATH, $"{key.Public.Type} {key.Public.Value}");
            await File.WriteAllTextAsync(PRIVATE_KEY_PATH, $"{key.Private.Type} {key.Private.Value}");
        }

        public string CleanupKeyString(string key) => key.Replace("\r", "");
    }
}