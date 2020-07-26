using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tides.Core;
using Tides.Models.Auth;
using Tides.Models.Auth.Types;
using Tides.Models.Crypto;
using Tides.Models.Public;
using Tides.Services;

namespace Tides.Models.Peer.Client
{
    public class HttpActivityStreamRepository: IActivityStreamRepository
    {
        private readonly KeyPair _keys;
        protected readonly Tides.Actors.Peer _peer;
        protected readonly HttpClientHandler _clientHandler;

        public HttpActivityStreamRepository(KeyPair keys, Tides.Actors.Peer peer)
        {
            _keys = keys;
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

        public async Task<HttpResponseMessage> DoRequest(HttpMethod method, string route, Stream body, Action<HttpRequestMessage> configure)
        {
            if (string.IsNullOrWhiteSpace(this._peer.Address?.ToString()))
            {
                throw new ArgumentNullException("_peer.Address"); // }:{_peer.AddressPort}
            }

            using (var client = GetClient())
            {
                client.BaseAddress = new Uri($"https://{_peer.Address}:{_peer.AddressPort}/");
                var request = new HttpRequestMessage(method, string.IsNullOrWhiteSpace(route) ? string.Empty : route);
                configure(request);

                if (_keys != null)
                {
                    if (_keys.Public != null)
                    {
                        var pub = _keys.Public.Value.Replace("\n", "%n%");
                        request.Headers.Add("X-Api-Key", new string[]{ $"{_keys.Public.Type} {pub}" });

                        if (body != null)
                        {
                            var helper = new AesHelper();
                            helper.Randomize();
                            request.Headers.Add("X-Api-Cipher", $"{helper.Size} {Convert.ToBase64String(helper.Key)}");
                        }
                    }
                }

                return await client.SendAsync(request);
            }
        }

        public async Task<T> HandleRequest<T>(HttpMethod method, string route, Stream body, Action<HttpRequestMessage> configure, Func<HttpResponseMessage, Task<T>> handler) => await handler(await DoRequest(method, route, body, configure));

        public async Task<string> QueryHtml(string route)
        {
            return await HandleRequest(HttpMethod.Get, route, null,
                    req => req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html")),
                    HandleHtmlResponse);
        }

        public async Task<string> QueryJson(string route)
        {
            return await HandleRequest<string>(HttpMethod.Get, route, null, 
                       req => req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")),
                        HandleTextResponse);
                        
                // request.Headers.Add("Content-Type", new string[]{ "application/json" });
        }

        private async Task<string> HandleHtmlResponse(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentType.MediaType == "text/html")
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            else
            {
                throw new Exception($"Invalid response type {response.Content.Headers.ContentType.MediaType}");
            }
        }

        public async Task<string> HandleTextResponse(HttpResponseMessage response)
        {
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
                                    var cipherHeader = response.Content.Headers.GetValues("X-Api-Cipher").First();
                                    var helper = new AesHelper();
                                    stream.Read(helper.IV);

                                    var cipherEncrypted = new MemoryStream(Convert.FromBase64String(cipherHeader));
                                    var cipherDecrypted = new MemoryStream();
                                    await new RSAAuthHandlerMsft().DecryptAndVerifyStreamAsync(cipherEncrypted, cipherDecrypted, _keys.Private, _keys.Public, null);
                                    
                                    helper.Decrypt(stream, outputStream);
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

        public Task<BaseObject> GetActor(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetActors(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetPost(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetPosts(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetReply(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetReplies(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetMentions(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetShop(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetShops(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetFollowers(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetFollowing(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetMedia(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetPeer(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetPeers(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> Get()
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> Write(BaseObject item)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetAll(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetFollowers(PublicId publicId, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetFollowing(PublicId publicId, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetInbox(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetInventories(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetInventory(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> GetResources(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<BaseObject> GetResource(ActivityStreamFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}