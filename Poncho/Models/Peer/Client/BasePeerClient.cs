using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Poncho.Models.Auth;
using Poncho.Models.Auth.Types;

namespace Poncho.Models.Peer.Client
{
    public abstract class BasePeerClient: IPeerClient
    {
        protected readonly PeerModel _peer;
        protected readonly HttpClientHandler _clientHandler;
        public IPeerConnectionType Type { get; }
        

        public BasePeerClient(IPeerConnectionType type, PeerModel peer)
        {
            Type = type;
            _peer = peer ?? throw new ArgumentNullException(nameof(peer));
            _clientHandler = new HttpClientHandler();
            if (true || System.Diagnostics.Debugger.IsAttached)
            {
                _clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    return true; 
                };
            }
        }

        protected HttpClient GetClient() => new HttpClient(_clientHandler, false);

        public async Task<string> QueryHtml(string route, KeyPair keys)
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

        public async Task<string> QueryJson(string route, KeyPair keys)
        {
            if (string.IsNullOrWhiteSpace(this._peer.Address?.ToString()))
            {
                throw new ArgumentNullException("_peer.Address"); // }:{_peer.AddressPort}
            }

            using (var client = GetClient())
            {
                client.BaseAddress = new Uri($"https://{_peer.Address}:{_peer.AddressPort}/");
                var request = new HttpRequestMessage(HttpMethod.Get, string.IsNullOrWhiteSpace(route) ? string.Empty : route);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // request.Headers.Add("Content-Type", new string[]{ "application/json" });
                if (keys != null)
                {
                    if (keys.Public != null)
                    {
                        var pub = keys.Public.Value.Replace("\n", "%n%").Replace("\r", "");
                        request.Headers.Add("X-Api-Key", new string[]{ $"{keys.Public.Type} {pub}" });
                    }
                }

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    if (response.Content.Headers.ContentLength.HasValue && response.Content.Headers.ContentLength.Value == 0)
                    {
                        return string.Empty;
                    }
                    using var responseBody = new MemoryStream();
                    await response.Content.CopyToAsync(responseBody);

                    if (responseBody.Position == 0)
                    {
                        return string.Empty;
                    }
                    responseBody.Position = 0;

                    switch (response.Content.Headers.ContentType.MediaType)
                    {
                        case "text/html": throw new Exception(await response.Content.ReadAsStringAsync());
                        case "application/json":
                            {
                                using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                                {
                                    return await reader.ReadToEndAsync();
                                }
                            }
                        case "application/pink-umbrella":
                            {
                                using (var stream = await response.Content.ReadAsStreamAsync())
                                {
                                    using (var outputStream = new MemoryStream())
                                    {
                                        await new RSAAuthHandler().DecryptAndVerifyStreamAsync(stream, outputStream, keys.Private, keys.Public, null);
                                        return System.Text.Encoding.UTF8.GetString(outputStream.ToArray());
                                    }
                                }
                            }
                        default: throw new Exception($"Invalid response type {response.Content.Headers.ContentType.MediaType}");
                    }
                }
                else
                {
                    throw new Exception($"Invalid response: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                }
            }
        }

        public abstract Task<PeerModel> Query(KeyPair keys);
        public abstract Task<List<string>> QueryMetaData(KeyPair keys);
        public abstract Task<PeerStatsModel> QueryStats(KeyPair keys);
        public abstract Task<object> QueryViewModel(string route, KeyPair keys);
    }
}