using System;
using System.Threading.Tasks;
using Nest;
using PinkUmbrella.Services.Elastic;
using Estuary.Core;
using Tides.Models.Public;

namespace PinkUmbrella.Services.NoSql
{
    public class ElasticService: BaseElasticService, IElasticService
    {
        public Task SyncProfile(long authId, BaseObject profile) => SyncProfile(GetClient(), authId, profile);

        public async Task SyncProfiles(long authId, CollectionObject profiles)
        {
            if (profiles == null || profiles.totalItems == 0)
            {
                Console.WriteLine($"No profiles to sync for {authId}");
                return;
            }

            var client = GetClient();

            foreach (var profile in profiles.items)
            {
                if (profile == null)
                {
                    continue;
                }
                await SyncProfile(client, authId, profile);
            }
        }

        private Task SyncProfile(ElasticClient client, long authId, BaseObject profile) => HandleSyncResponse(profile, client.IndexAsync(profile, i => i.Index("profiles")));

        public async Task SyncPosts(long peerId, CollectionObject items)
        {
            if (items == null || items.totalItems == 0)
            {
                Console.WriteLine($"No posts to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items.items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncPost(client, peerId, item);
            }
        }

        public Task SyncPost(long peerId, BaseObject item) => SyncPost(GetClient(), peerId, item);

        private Task SyncPost(ElasticClient client, long peerId, BaseObject item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("posts")));

        public async Task SyncShops(long peerId, CollectionObject items)
        {
            if (items == null || items.totalItems == 0)
            {
                Console.WriteLine($"No shops to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items.items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncShop(client, peerId, item);
            }
        }

        public Task SyncShop(long peerId, BaseObject item) => SyncShop(GetClient(), peerId, item);

        private Task SyncShop(ElasticClient client, long peerId, BaseObject item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("shops")));

        public async Task SyncArchivedMedias(long peerId, CollectionObject items)
        {
            if (items == null || items.totalItems == 0)
            {
                Console.WriteLine($"No archived media to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items.items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncArchivedMedia(client, peerId, item);
            }
        }

        public Task SyncArchivedMedia(long peerId, BaseObject item) => SyncArchivedMedia(GetClient(), peerId, item);

        private Task SyncArchivedMedia(ElasticClient client, long peerId, BaseObject item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("media-archive")));

        public async Task SyncPeers(long peerId, CollectionObject items)
        {
            if (items == null || items.totalItems == 0)
            {
                Console.WriteLine($"No peers to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items.items)
            {
                if (item == null)
                {
                    continue;
                }
                // await SyncArchivedMedia(client, peerId, item);
            }
            await Task.Delay(1);
        }

        public async Task SyncInventories(long peerId, CollectionObject items)
        {
            if (items == null || items.totalItems == 0)
            {
                Console.WriteLine($"No intentories to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items.items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncInventory(client, peerId, item);
            }
        }

        public Task SyncInventory(long peerId, BaseObject media)
        {
            throw new NotImplementedException();
        }
        
        private Task SyncInventory(ElasticClient client, long peerId, BaseObject item) => HandleSyncResponse(item, client.IndexAsync(item, i => i.Index("inventories")));





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

        public Task SyncPeer(long peerId, BaseObject items)
        {
            throw new NotImplementedException();
        }

        public async Task SyncObjects(long peerId, CollectionObject items)
        {
            if (items == null || items.totalItems == 0)
            {
                Console.WriteLine($"No objects to sync for {peerId}");
                return;
            }

            var client = GetClient();

            foreach (var item in items.items)
            {
                if (item == null)
                {
                    continue;
                }
                await SyncObject(client, peerId, item);
            }
        }

        public Task SyncObject(long peerId, BaseObject item) => SyncObject(GetClient(), peerId, item);

        private Task SyncObject(ElasticClient client, long peerId, BaseObject item) => HandleSyncResponse(item, client.IndexAsync<BaseObject>(item, i => i.Id(item.id))); // , i => i.Index() $"{item.type}-{}"
    }
}