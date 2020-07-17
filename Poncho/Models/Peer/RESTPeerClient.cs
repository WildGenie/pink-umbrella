using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Poncho.Models.Auth;
using Poncho.Models.Peer.Client;

namespace Poncho.Models.Peer
{
    public class RESTPeerClient : BasePeerClient
    {
        public RESTPeerClient(IPeerConnectionType type, PeerModel peer): base(type, peer) { }

        public override async Task<PeerModel> Query(KeyPair keys) => await JsonQuery<PeerModel>("Api/System/Index", keys);

        public override async Task<PeerStatsModel> QueryStats(KeyPair keys) => await JsonQuery<PeerStatsModel>("Api/System/Stats", keys);

        public override Task<List<string>> QueryMetaData(KeyPair keys)
        {
            throw new NotImplementedException();
        }

        public override Task<object> QueryViewModel(string route, KeyPair keys)
        {
            throw new NotImplementedException();
        }

        public async Task<T> JsonQuery<T>(string route, KeyPair keys)
        {
            var json = await QueryJson(route, keys);
            if (!string.IsNullOrWhiteSpace(json))
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            return default;
        }

        public async Task<List<PeerModel>> GetPeers(DateTime? sinceLastUpdated, KeyPair keys)
        {
            var query = sinceLastUpdated.HasValue ? $"Api/Peer/All?sinceLastUpdated={sinceLastUpdated.Value.Ticks}" : "Api/Peer/All";
            var response = await JsonQuery<ListResultModel<PeerModel>>(query, keys);
            return response?.items;
        }
    }

    public class ListResultModel<T>
    {
        public List<T> items { get; set; }
    }
}