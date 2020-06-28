using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Peer;
using PinkUmbrella.Util;

namespace PinkUmbrella.Services.NoSql
{
    public class PeerService : IPeerService
    {
        private readonly StringRepository _strings;
        private readonly IPeerConnectionTypeResolver _typeResolver;
        private readonly ExternalDbOptions _options;

        public PeerService(StringRepository strings, IPeerConnectionTypeResolver typeResolver, ExternalDbOptions options)
        {
            _strings = strings;
            _typeResolver = typeResolver;
            _options = options;
        }

        public async Task<IPeerClient> Open(string handle)
        {
            var peer = await GetPeer(handle);
            DbContext db = null;
            if (peer != null)
            {
                db = await _options.OpenDbContext.Invoke(handle);
            }
            else
            {
                peer = new PeerModel()
                {
                    Handle = handle,
                };
            }
            return _typeResolver.Get(PeerConnectionType.RestApiV1).Open(peer, db);
        }

        public async Task<List<PeerModel>> GetPeers()
        {
            var ret = new List<PeerModel>();
            var handles = System.IO.Directory.GetFiles("Peers");
            foreach (var handle in handles)
            {
                ret.Add(await GetPeer(handle));
            }
            return ret;
        }

        public async Task AddPeer(PeerModel peer)
        {
            if (_strings.ValidHandleRegex.IsMatch(peer.Handle))
            {
                using (var stream = new FileStream($"Peers/{peer.Handle}/peer.json", FileMode.CreateNew))
                {
                    await JsonSerializer.SerializeAsync(stream, peer);
                }
            }
            else
            {
                throw new ArgumentException("Peer handle is invalid", nameof(peer.Handle));
            }
        }

        public async Task<PeerModel> GetPeer(string handle)
        {
            if (System.IO.Directory.Exists($"Peers/{handle}") && _strings.ValidHandleRegex.IsMatch(handle))
            {
                PeerModel peer = null;
                using (var stream = new FileStream($"Peers/{handle}/peer.json", FileMode.Open))
                {
                    peer = await JsonSerializer.DeserializeAsync<PeerModel>(stream);
                }

                // using (var stream = new FileStream($"Peers/{handle}/peer_meta.json", FileMode.Open))
                // {
                //     await _typeResolver.ResolveMetaData(peer, stream, PeerConnectionType.RestApiV1);
                // }

                return peer;
            }
            else
            {
                return null;
            }
        }

        public Task RemovePeer(PeerModel peer)
        {
            if (_strings.ValidHandleRegex.IsMatch(peer.Handle))
            {
                System.IO.Directory.Delete($"Peers/{peer.Handle}");
            }
            return Task.CompletedTask;
        }

        public async Task ReplacePeer(PeerModel peer)
        {
            await RemovePeer(peer);
            await AddPeer(peer);
        }

        public Task RenamePeer(string handleFrom, string handleTo)
        {
            if (_strings.ValidHandleRegex.IsMatch(handleFrom) && _strings.ValidHandleRegex.IsMatch(handleTo))
            {
                System.IO.Directory.Move($"Peers/{handleFrom}", $"Peers/{handleTo}");
                return Task.CompletedTask;
            }
            else
            {
                throw new ArgumentException("Handle is invalid", nameof(handleFrom));
            }
        }
    }
}