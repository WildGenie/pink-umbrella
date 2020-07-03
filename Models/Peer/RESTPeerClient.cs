using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Elastic;
using PinkUmbrella.Services.Peer;

namespace PinkUmbrella.Models.Peer
{
    public class RESTPeerClient : IPeerClient
    {
        private readonly PeerModel _peer;
        private readonly DbContext _db;
        private readonly HttpClientHandler _clientHandler;

        public IPeerConnectionType Type { get; }

        public RESTPeerClient(IPeerConnectionType type, PeerModel peer, DbContext context)
        {
            Type = type;
            _peer = peer ?? throw new ArgumentNullException(nameof(peer));
            _db = context;
            _clientHandler = new HttpClientHandler();
            if (System.Diagnostics.Debugger.IsAttached)
            {
                _clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }
        }

        public async Task<PeerModel> Query() => await JsonQuery<PeerModel>("Api/System");

        public async Task<PeerStatsModel> QueryStats() => await JsonQuery<PeerStatsModel>("Api/System/Stats");

        private HttpClient GetClient() => new HttpClient(_clientHandler, false);

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

        public Task<List<string>> QueryMetaData()
        {
            throw new NotImplementedException();
        }

        public async Task<string> QueryHtml(string route)
        {
            string ret = null;
            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                var url = $"https://{_peer.Address}:{_peer.AddressPort}/{route}";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    switch (response.Content.Headers.ContentType.MediaType)
                    {
                        case "application/json": throw new Exception(await response.Content.ReadAsStringAsync());
                        case "text/html":
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    ret = await reader.ReadToEndAsync();
                                }
                            }
                        }
                        break;
                        default: throw new Exception($"Invalid response type {response.Content.Headers.ContentType.MediaType}");
                    }
                }
            }
            return ret;
        }

        public Task<object> QueryViewModel(string route)
        {
            throw new NotImplementedException();
        }

        private async Task<T> JsonQuery<T>(string route)
        {
            using (var client = GetClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var url = $"https://{_peer.Address}:{_peer.AddressPort}/{route}";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    switch (response.Content.Headers.ContentType.MediaType)
                    {
                        case "text/html": throw new Exception(await response.Content.ReadAsStringAsync());
                        case "application/json":
                            {
                                using (var stream = await response.Content.ReadAsStreamAsync())
                                {
                                    return await JsonSerializer.DeserializeAsync<T>(stream);
                                }
                            }
                        default: throw new Exception($"Invalid response type {response.Content.Headers.ContentType.MediaType}");
                    }
                }
                else
                {
                    return default;
                }
            }
        }

        public async Task<List<ElasticProfileModel>> GetProfiles(DateTime? sinceLastUpdated)
        {
            var query = sinceLastUpdated.HasValue ? $"Api/Profile/All?sinceLastUpdated={sinceLastUpdated.Value.Ticks}" : "Api/Profile/All";
            var ret = (await JsonQuery<ProfilesModel>(query))?.profiles;
            if (ret != null)
            {
                foreach (var r in ret)
                {
                    if (this._peer.PublicKey == null)
                    {
                        r.AuthId = this._peer.PublicKey.Id;
                    }
                    r.SetIdFromCompound();
                }
            }
            return ret;
        }
    }

    public class ProfilesModel
    {
        public List<ElasticProfileModel> profiles { get; set; }
    }
}