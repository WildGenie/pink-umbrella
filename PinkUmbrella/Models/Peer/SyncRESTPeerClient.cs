using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models.Public;
using Tides.Models.Auth;
using Tides.Models.Peer;

namespace PinkUmbrella.Models.Peer
{
    public class SyncRESTPeerClient: RESTPeerClient, ISyncPeerClient
    {
        public SyncRESTPeerClient(IPeerConnectionType type, PeerModel peer) : base(type, peer)
        {
        }

        public async Task<List<PublicProfileModel>> GetProfiles(DateTime? sinceLastUpdated, KeyPair keys)
        {
            var query = sinceLastUpdated.HasValue ? $"Api/Profile/All?sinceLastUpdated={sinceLastUpdated.Value.Ticks}" : "Api/Profile/All";
            var ret = (await JsonQuery<ListResultModel<PublicProfileModel>>(query, keys))?.items;
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

        public async Task<List<PostModel>> GetPosts(DateTime? sinceLastUpdated, KeyPair keys)
        {
            var query = sinceLastUpdated.HasValue ? $"Api/Post/All?sinceLastUpdated={sinceLastUpdated.Value.Ticks}" : "Api/Post/All";
            var ret = (await JsonQuery<ListResultModel<PostModel>>(query, keys))?.items;
            if (ret != null)
            {
                foreach (var r in ret)
                {
                    if (this._peer.PublicKey == null)
                    {
                        r.PeerId = this._peer.PublicKey.Id;
                    }
                }
            }
            return ret;
        }

        public async Task<List<ShopModel>> GetShops(DateTime? sinceLastUpdated, KeyPair keys)
        {
            var query = sinceLastUpdated.HasValue ? $"Api/Shop/All?sinceLastUpdated={sinceLastUpdated.Value.Ticks}" : "Api/Shop/All";
            var ret = (await JsonQuery<ListResultModel<ShopModel>>(query, keys))?.items;
            if (ret != null)
            {
                foreach (var r in ret)
                {
                    if (this._peer.PublicKey == null)
                    {
                        r.PeerId = this._peer.PublicKey.Id;
                    }
                }
            }
            return ret;
        }

        public async Task<List<ArchivedMediaModel>> GetArchivedMedia(DateTime? sinceLastUpdated, KeyPair keys)
        {
            var query = sinceLastUpdated.HasValue ? $"Api/Archive/All?sinceLastUpdated={sinceLastUpdated.Value.Ticks}" : "Api/Archive/All";
            var ret = (await JsonQuery<ListResultModel<ArchivedMediaModel>>(query, keys))?.items;
            if (ret != null)
            {
                foreach (var r in ret)
                {
                    if (this._peer.PublicKey == null)
                    {
                        r.PeerId = this._peer.PublicKey.Id;
                    }
                }
            }
            return ret;
        }

        public async Task<List<SimpleInventoryModel>> GetInventories(DateTime? sinceLastUpdated, KeyPair keys)
        {
            var query = sinceLastUpdated.HasValue ? $"Api/Inventory/All?sinceLastUpdated={sinceLastUpdated.Value.Ticks}" : "Api/Inventory/All";
            var ret = (await JsonQuery<ListResultModel<SimpleInventoryModel>>(query, keys))?.items;
            if (ret != null)
            {
                foreach (var r in ret)
                {
                    if (this._peer.PublicKey == null)
                    {
                        r.PeerId = this._peer.PublicKey.Id;
                    }
                }
            }
            return ret;
        }
    }
}