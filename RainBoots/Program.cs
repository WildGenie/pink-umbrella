using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Poncho.Models.Auth;
using Poncho.Models.Auth.Types;
using Poncho.Models.Peer;

namespace RainBoots
{
    class Program
    {
        private static readonly string PUBLIC_KEY_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ssh/id_pink.pub";
        
        private static readonly string PRIVATE_KEY_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.ssh/id_pink";

        private static bool HAS_PUBLIC_KEY => File.Exists(PUBLIC_KEY_PATH);

        private static string PUBLIC_KEY => File.ReadAllText(PUBLIC_KEY_PATH).Substring(4);
        
        private static string PRIVATE_KEY => File.ReadAllText(PRIVATE_KEY_PATH).Substring(4);

        private static KeyPair MyKeys => new KeyPair { Public = publicKey, Private = privateKey };

        private static RESTPeerClient client = null;
        private static string serverUrl = "localhost";
        private static int serverPort = 12524;

        private static PublicKey publicKey => new PublicKey { Value = PUBLIC_KEY, Type = AuthType.RSA };
        
        private static PrivateKey privateKey => new PrivateKey { Value = PRIVATE_KEY, Type = AuthType.RSA };

        static void Main(string[] args)
        {
            InitKeys();
            RefreshClient();

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var boots = new Boots();
            boots.Add("config",         "Manage program settings",                  new Regex(@"(?<url>url=[a-zA-Z0-9./\:;-]+)|(?<port>port=\d+)|(?<refresh>refresh)"), Config);
            boots.Add("api",            "Get API information from server",          new Regex(@"(?<filter>.*)"), ListApi);
            boots.Add("auth",           "Manage credentials for authentication",    new Regex(@"(?<recycle>recycle)|(?<show>show)(?: *)(?<which>.*)|(?<add>add)(?: *)(?<which>.*)|(?<remove>remove)(?: *)(?<which>.*)|(?<help>help)(?: *)(?<which>.*)"), Auth);
            boots.Add("get",            "Get peer information from server",         new Regex(@"(?<route>.*)"), DoGet);
            boots.Add("json",           "Perform API action with JSON response",    new Regex(@"(?<route>.*)"), DoJson);
            boots.Add("html",           "View page on server",                      new Regex(@"(?<route>.*)"), DoHtml);
            boots.Add("trust",          "Trust server",                             new Regex(@"(?<route>.*)"), Trust);
            
            boots.Loop(() => {
                Console.Write("> ");
                return Console.In.ReadLine();
            }, content => Console.WriteLine(content)).Wait();
        }

        private static async Task<string> ListApi(Match arg)
        {
            var route = "Api/System/Api";
            if (arg.Groups.TryGetValue("filter", out var filterMatch) && filterMatch.Value.Length > 0)
            {
                route += $"?filter={Uri.EscapeDataString(filterMatch.Value)}";
            }
            var res = (await client.QueryJson(route, MyKeys)) ?? "{ \"error\": \"no response\" }";
            return $"application/json {res}";
        }

        private static async Task<string> Trust(Match arg)
        {
            var res = await client.Query(MyKeys);
            
            return $"application/json {res}";
        }

        private static async void InitKeys()
        {
            if (!HAS_PUBLIC_KEY)
            {
                var rsa = await new RSAAuthHandler().GenerateKey(AuthKeyFormat.Raw, HandshakeMethod.Default);
                await File.WriteAllTextAsync(PUBLIC_KEY_PATH, $"{rsa.Public.Type} {rsa.Public.Value}");
                await File.WriteAllTextAsync(PRIVATE_KEY_PATH, $"{rsa.Private.Type} {rsa.Private.Value}");

                Console.WriteLine($"You should add your public key to the server you wish to connect to:");
                Console.WriteLine(PUBLIC_KEY);
            }
        }

        private static async Task<string> DoGet(Match arg)
        {
            return (await client.Query(MyKeys))?.ToString() ?? "no response";
        }

        private static async Task<string> DoHtml(Match arg)
        {
            var html = await client.QueryHtml("", MyKeys);
            return $"text/html {html}";
        }

        private static async Task<string> DoJson(Match arg)
        {
            var route = "Api/System/Index";
            if (arg.Groups.TryGetValue("route", out var routeMatch) && routeMatch.Value.Length > 0)
            {
                route = routeMatch.Value;
            }
            var res = (await client.QueryJson(route, MyKeys)) ?? "{ \"error\": \"no response\" }";
            return $"application/json {res}";
        }

        private static Task<string> Config(Match arg)
        {
            if (arg.Groups.TryGetValue("url", out var url))
            {
                serverUrl = url.Value;
            }
            else if (arg.Groups.TryGetValue("port", out var port))
            {
                serverPort = int.Parse(port.Value);
            }
            else if (arg.Groups.TryGetValue("refresh", out var refresh)) { }
            else
            {
                return Task.FromResult("error Invalid usage.");
            }
            
            RefreshClient();
            return Task.FromResult("text/plain ok");
        }

        private static Task<string> Auth(Match arg)
        {
            if (arg.Groups.TryGetValue("recycle", out var recycle) && recycle.Value.Length > 0)
            {
                if (HAS_PUBLIC_KEY)
                {
                    Console.WriteLine($"Deleting old keys");
                    File.Delete(PUBLIC_KEY_PATH);
                    File.Delete(PRIVATE_KEY_PATH);
                }
                InitKeys();
            }
            else if (arg.Groups.TryGetValue("help", out var help) && help.Value.Length > 0)
            {
                var topic = string.Empty;
                if (arg.Groups.TryGetValue("which", out var which2show) && which2show.Value.Length > 0)
                {
                    topic = which2show.Value;
                }

                switch (topic)
                {
                    case "show": return Task.FromResult("text/plain Lists the authentication keys for this client");
                    case "recycle": return Task.FromResult("text/plain Regenerates the authentication keys for this client");
                    default: return Task.FromResult("text/plain Use auth help show or auth help recycle");
                }
            }
            else if (arg.Groups.TryGetValue("show", out var show) && show.Value.Length > 0)
            {
                var sb = new StringBuilder();
                sb.Append("my public: \t");
                sb.Append(PUBLIC_KEY);
                sb.Append("\n\n");
                sb.Append("my private: \t");
                sb.Append(PRIVATE_KEY);
                
                return Task.FromResult($"text/plain {sb}");
            }
            else
            {
                return Task.FromResult("error Invalid usage. Use auth help to show usage.");
            }
            
            return Task.FromResult("text/plain ok");
        }

        private static void RefreshClient()
        {
            client = new RESTPeerClient(new RESTPeerClientType(), new PeerModel() { Address = new IPAddressModel { Address = serverUrl, Name = serverUrl }, AddressPort = serverPort });
        }

        private static string GetPassword()
        {
            Console.Write("Password: ");
            return Console.In.ReadLine();
        }
    }
}
