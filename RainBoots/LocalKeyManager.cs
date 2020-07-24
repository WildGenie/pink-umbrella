using System;
using Tides.Models.Auth;
using Tides.Models.Auth.Types;
using Tides.Models.Crypto;

namespace RainBoots
{
    public class LocalKeyManager: BaseKeyManager
    {
        public async void Generate()
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
            await Generate(handler);
        }
    }
}