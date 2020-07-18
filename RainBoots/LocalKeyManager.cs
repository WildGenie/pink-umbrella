using System;
using System.IO;
using Poncho.Models.Auth;
using Poncho.Models.Auth.Types;

namespace RainBoots
{
    public class LocalKeyManager
    {
        public static readonly string PUBLIC_KEY_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ssh/id_pink.pub";
        
        public static readonly string PRIVATE_KEY_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ssh/id_pink";

        public static string PUBLIC_KEY => ReadKey(PUBLIC_KEY_PATH);

        public static string PRIVATE_KEY=> ReadKey(PRIVATE_KEY_PATH);
        
        public static AuthType PUBLIC_KEY_TYPE => ReadKeyType(PUBLIC_KEY_PATH);
        
        public static AuthType PRIVATE_KEY_TYPE => ReadKeyType(PRIVATE_KEY_PATH);

        private static string ReadKey(string path)
        {
            var key = File.ReadAllText(path);
            var sep = key.IndexOf(' ');
            return key.Substring(sep + 1);
        }

        private static AuthType ReadKeyType(string path)
        {
            var buffer = new byte[32];
            using var fileReader = File.OpenRead(path);
            fileReader.Read(buffer);
            var str = System.Text.Encoding.UTF8.GetString(buffer);
            var sep = str.IndexOf(' ');
            return (AuthType) Enum.Parse(typeof(AuthType), str.Substring(0, sep));
        }

        public static KeyPair MyKeys => new KeyPair { Public = publicKey, Private = privateKey };

        public static PublicKey publicKey => new PublicKey { Value = PUBLIC_KEY, Type = PUBLIC_KEY_TYPE, Format = AuthKeyFormatResolver.Resolve(PUBLIC_KEY).Value };
        
        public static PrivateKey privateKey => new PrivateKey { Value = PRIVATE_KEY, Type = PRIVATE_KEY_TYPE, Format = AuthKeyFormatResolver.Resolve(PRIVATE_KEY).Value };

        public static bool Exists => File.Exists(PUBLIC_KEY_PATH);

        public static void Delete()
        {
            File.Delete(PUBLIC_KEY_PATH);
            File.Delete(PRIVATE_KEY_PATH);
        }

        public static async void Generate()
        {
            Console.Write("Key type (pgp or rsa) and format (msft or pem): ");
            var line = Console.In.ReadLine().Split(' ');

            IAuthTypeHandler handler = null;
            switch (line[0])
            {
                case "rsa":
                switch (line[1])
                {
                    case "msft": handler = new RSAAuthHandlerMsft(); break;
                    case "pem": handler = new RSAAuthHandlerBouncy(); break;
                    default:
                        Console.WriteLine("Invalid key format");
                    return;
                }
                break;
                case "pgp":
                switch (line[1])
                {
                    case "pem": handler = new OpenPGPAuthHandler(); break;
                    default:
                        Console.WriteLine("Invalid key format (only pem)");
                    return;
                }
                break;
                default:
                    Console.WriteLine("Invalid key type");
                return;
            }

            var key = await handler.GenerateKey(HandshakeMethod.Default);
            await File.WriteAllTextAsync(PUBLIC_KEY_PATH, $"{key.Public.Type} {key.Public.Value}");
            await File.WriteAllTextAsync(PRIVATE_KEY_PATH, $"{key.Private.Type} {key.Private.Value}");
        }
    }
}