using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Models.Public;
using PinkUmbrella.Services.Elastic;

namespace PinkUmbrella.Services.NoSql
{
    public class ElasticService: BaseElasticService, IElasticService
    {
        public Task SyncProfile(long authId, PublicProfileModel profile) => SyncProfile(GetClient(), authId, profile);

        public async Task SyncProfiles(long authId, List<PublicProfileModel> profiles)
        {
            if (profiles == null || profiles.Count == 0)
            {
                Console.WriteLine($"No profiles to sync for {authId}");
                return;
            }

            var client = GetClient();

            foreach (var profile in profiles)
            {
                if (profile == null)
                {
                    continue;
                }
                await SyncProfile(client, authId, profile);
            }
        }

        private Task SyncProfile(ElasticClient client, long authId, PublicProfileModel profile) => HandleSyncResponse(profile, client.IndexAsync(profile, i => i.Index("profiles")));

        public async Task SyncPosts(long peerId, List<PostModel> items)
        {
            if (items == null || items.Count == 0)
            {
                Console.WriteLine($"No posts to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncPost(client, peerId, item);
            }
        }

        public Task SyncPost(long peerId, PostModel item) => SyncPost(GetClient(), peerId, item);

        private Task SyncPost(ElasticClient client, long peerId, PostModel item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("posts")));

        public async Task SyncShops(long peerId, List<ShopModel> items)
        {
            if (items == null || items.Count == 0)
            {
                Console.WriteLine($"No shops to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncShop(client, peerId, item);
            }
        }

        public Task SyncShop(long peerId, ShopModel item) => SyncShop(GetClient(), peerId, item);

        private Task SyncShop(ElasticClient client, long peerId, ShopModel item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("shops")));

        public async Task SyncArchivedMedias(long peerId, List<ArchivedMediaModel> items)
        {
            if (items == null || items.Count == 0)
            {
                Console.WriteLine($"No archived media to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncArchivedMedia(client, peerId, item);
            }
        }

        public Task SyncArchivedMedia(long peerId, ArchivedMediaModel item) => SyncArchivedMedia(GetClient(), peerId, item);

        private Task SyncArchivedMedia(ElasticClient client, long peerId, ArchivedMediaModel item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("media-archive")));

        public async Task SyncPeers(long peerId, List<PeerModel> items)
        {
            if (items == null || items.Count == 0)
            {
                Console.WriteLine($"No peers to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                // await SyncArchivedMedia(client, peerId, item);
            }
        }

        public async Task SyncInventories(long peerId, List<SimpleInventoryModel> items)
        {
            if (items == null || items.Count == 0)
            {
                Console.WriteLine($"No intentories to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncInventory(client, peerId, item);
            }
        }

        public Task SyncInventory(long peerId, SimpleInventoryModel media)
        {
            throw new NotImplementedException();
        }
        
        private Task SyncInventory(ElasticClient client, long peerId, SimpleInventoryModel item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("inventories")));





        private static async Task HandleSyncResponse(IHazPublicId id, Task<IndexResponse> resultTask)
        {
            var result = await resultTask;
            if (result.Result == Result.Created || result.Result == Result.Updated)
            {
                Console.WriteLine($"{result.Result} {id.PublicId} on {result.Index}");
            }
            else
            {
                Console.WriteLine($"Error adding {id.PublicId} on {result.Index}: {result}");
                if (result.OriginalException != null)
                {
                    Console.WriteLine(result.OriginalException);
                }
            }

            await Task.Delay(1000);
        }
    }
}