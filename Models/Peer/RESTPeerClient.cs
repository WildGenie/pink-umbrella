using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Models.Peer
{
    public class RESTPeerClient : IPeerClient
    {
        private readonly string _handle;
        private readonly HttpClientHandler _clientHandler;

        public IPeerConnectionType Type { get; }

        public RESTPeerClient(IPeerConnectionType type, string handle)
        {
            Type = type;
            _handle = handle;
            _clientHandler = new HttpClientHandler();
            if (System.Diagnostics.Debugger.IsAttached)
            {
                _clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }
        }

        public async Task<PeerModel> Query()
        {
            // Pass the handler to httpclient(from you are calling api)
            using (var client = new HttpClient(_clientHandler))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync($"{_handle}/Api/System");
                if (response.IsSuccessStatusCode)
                {
                    switch (response.Content.Headers.ContentType.MediaType)
                    {
                        case "text/html": throw new Exception(await response.Content.ReadAsStringAsync());
                        case "application/json":
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                return await JsonSerializer.DeserializeAsync<PeerModel>(stream);
                            }
                        }
                        default: throw new Exception($"Invalid response type {response.Content.Headers.ContentType.MediaType}");
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        // https://stackoverflow.com/a/20097642/11765486
        /// <summary>
        /// Certificate validation callback.
        /// </summary>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // throw new Exception(string.Format("X509Certificate [{0}] Policy Error: '{1}'",
            //     cert.Subject,
            //     error.ToString()));

            return false;
        }

        public Task<PeerModelMetaData> QueryMetaData()
        {
            throw new NotImplementedException();
        }
    }
}