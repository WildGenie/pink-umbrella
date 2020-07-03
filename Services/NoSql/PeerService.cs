using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Auth;
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

        public async Task<IPeerClient> Open(IPAddressModel address, int? port = null)
        {
            var peer = await GetPeer(address, port);
            DbContext db = null;
            if (peer != null)
            {
                db = await _options.OpenDbContext.Invoke(PeerHandle(address, port ?? 443));
            }
            else
            {
                peer = new PeerModel()
                {
                    Address = address,
                    AddressPort = port ?? 443,
                };
            }
            return _typeResolver?.Get(PeerConnectionType.RestApiV1)?.Open(peer, db);
        }

        public async Task<List<PeerModel>> GetPeers()
        {
            var ret = new List<PeerModel>();
            var handles = System.IO.Directory.GetFileSystemEntries("Peers");
            foreach (var handle in handles)
            {
                var split = handle.Substring("Peers/".Length).Split('-');
                if (split.Length != 2)
                {
                    continue;
                }

                var address = split[0];
                var port = int.Parse(split[1]);
                ret.Add(await GetPeer(new IPAddressModel()
                {
                    Address = address,
                    Type = address.StartsWith('[') ? IPType.IPv6 : IPType.IPv4,
                }, port));
            }
            return ret;
        }

        public async Task AddPeer(PeerModel peer)
        {
            if (_strings.ValidHandleRegex.IsMatch(peer.DisplayName))
            {
                var dir = PeerDirectory(peer.Address, peer.AddressPort);
                System.IO.Directory.CreateDirectory(dir);
                using (var stream = new FileStream($"{dir}/peer.json", FileMode.CreateNew))
                {
                    await JsonSerializer.SerializeAsync(stream, peer);
                }
            }
            else
            {
                throw new ArgumentException("Peer display name is invalid", nameof(peer.DisplayName));
            }
        }

        public async Task<PeerModel> GetPeer(IPAddressModel address, int? port = null)
        {
            port = port ?? 443;
            var dir = PeerDirectory(address, port);
            if (System.IO.Directory.Exists(dir))
            {
                PeerModel peer = null;
                using (var stream = new FileStream($"{dir}/peer.json", FileMode.Open))
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

        private string PeerDirectory(IPAddressModel address, int? port) => $"Peers/{address}-{port}";
        
        private string PeerHandle(IPAddressModel address, int? port) => $"{address}-{port}";

        public Task RemovePeer(PeerModel peer)
        {
            var dir = PeerDirectory(peer.Address, peer.AddressPort);
            if (System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.Delete(dir);
            }
            return Task.CompletedTask;
        }

        public async Task ReplacePeer(PeerModel peer)
        {
            await RemovePeer(peer);
            await AddPeer(peer);
        }

        public Task PeerAddressChanged(IPAddressModel addressFrom, int portFrom, IPAddressModel addressTo, int portTo)
        {
            var dirFrom = PeerDirectory(addressFrom, portFrom);
            if (System.IO.Directory.Exists(dirFrom))
            {
                var dirTo = PeerDirectory(addressTo, portTo);
                if (!System.IO.Directory.Exists(dirTo))
                {
                    System.IO.Directory.Move(dirFrom, dirTo);
                    return Task.CompletedTask;
                }
                else
                {
                    return Task.FromException(new Exception($"New address and port already in use"));
                }
            }
            else
            {
                return Task.FromException(new Exception($"Existing address and port does not exist"));
            }
        }

        public Task<int> CountAsync() => Task.FromResult(System.IO.Directory.GetFileSystemEntries("Peers").Length);
    }
}