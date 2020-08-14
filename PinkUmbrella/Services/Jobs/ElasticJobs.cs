using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Estuary.Core;
using Estuary.Services;
using Estuary.Util;

namespace PinkUmbrella.Services.Jobs
{
    public static class ElasticJobs
    {
        public static IServiceProvider Services { get; set; }

        public static DateTime? LastTimeSyncedObjects { get; set; }

        // public static DateTime? LastTimeSyncedPosts { get; set; }

        // public static DateTime? LastTimeSyncedShops { get; set; }

        // public static DateTime? LastTimeSyncedArchivedMedia { get; set; }

        // public static DateTime? LastTimeSyncedPeers { get; set; }


        [AutomaticRetry(Attempts = 0)]
        public static async Task SyncObjects(IServiceProvider s)
        {
            var filter = new ActivityStreamFilter("outbox") { sinceLastUpdated = LastTimeSyncedObjects }.FixObjType("Actor");
            using (var scope = Services.CreateScope())
            {
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
                var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var peers = await peerService.GetPeers();
                if (peers.Count > 0)
                {
                    foreach (var peer in peers)
                    {
                        if (peer != null)
                        {
                            // await authService.GetKeyPair(peer.PublicKey)
                            var client = await peerService.Open(peer.Address, peer.AddressPort);
                            try
                            {
                                var profiles = await client.Get(filter);
                                if (profiles is OrderedCollectionObject orderedCollection)
                                {
                                    await elastic.SyncProfiles(peer.PublicKey.Id, orderedCollection);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error: {e.Message}");
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No peers");
                }
                
                var locals = await scope.ServiceProvider.GetRequiredService<IActivityStreamRepository>().Get(filter);
                if (locals is CollectionObject collection)
                {
                    await elastic.SyncObjects(0, collection);
                }
            }
            LastTimeSyncedObjects = DateTime.UtcNow;
        }

        // public static async Task SyncPosts(ElasticRunner applicationServices)
        // {
        //     using (var scope = Services.CreateScope())
        //     {
        //         var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
        //         var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
        //         var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        //         var peers = await peerService.GetPeers();
        //         if (peers.Count > 0)
        //         {
        //             var filter = new ActivityStreamFilter { sinceLastUpdated = LastTimeSyncedPosts };
        //             foreach (var peer in peers)
        //             {
        //                 if (peer != null)
        //                 {
        //                     var client = await peerService.Open(peer.Address, peer.AddressPort);
        //                     // await authService.GetKeyPair(peer.PublicKey)
        //                     try
        //                     {
        //                         var items = await client.GetPosts(filter);
        //                         //await elastic.SyncPosts(peer.PublicKey.Id, items);
        //                     }
        //                     catch (Exception e)
        //                     {
        //                         Console.WriteLine($"Error: {e.Message}");
        //                         Console.WriteLine(e);
        //                     }
        //                 }
        //             }
        //             LastTimeSyncedPosts = DateTime.UtcNow;
        //         }
        //         else
        //         {
        //             Console.WriteLine("No peers");
        //         }

        //         var locals = await scope.ServiceProvider.GetRequiredService<IActivityStreamRepository>().GetAllLocal();
        //         await elastic.SyncPosts(0, locals);
        //     }
        // }

        // public static async Task SyncMedia(ElasticRunner applicationServices)
        // {
        //     using (var scope = Services.CreateScope())
        //     {
        //         var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
        //         var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
        //         var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        //         var peers = await peerService.GetPeers();
        //         if (peers.Count > 0)
        //         {
        //             var filter = new ActivityStreamFilter { sinceLastUpdated = LastTimeSyncedArchivedMedia };
        //             foreach (var peer in peers)
        //             {
        //                 if (peer != null)
        //                 {
        //                     var client = await peerService.Open(peer.Address, peer.AddressPort);
        //                     // , await authService.GetKeyPair(peer.PublicKey)
        //                     try
        //                     {
        //                         var medias = await client.GetMedia(filter);
        //                         // await elastic.SyncArchivedMedias(peer.PublicKey.Id, medias);
        //                     }
        //                     catch (Exception e)
        //                     {
        //                         Console.WriteLine($"Error: {e.Message}");
        //                         Console.WriteLine(e);
        //                     }
        //                 }
        //             }
        //             LastTimeSyncedArchivedMedia = DateTime.UtcNow;
        //         }
        //         else
        //         {
        //             Console.WriteLine("No peers");
        //         }

        //         var locals = await scope.ServiceProvider.GetRequiredService<IArchiveService>().GetAllLocal();
        //         await elastic.SyncArchivedMedias(0, locals);
        //     }
        // }

        // public static async Task SyncShops(ElasticRunner applicationServices)
        // {
        //     using (var scope = Services.CreateScope())
        //     {
        //         var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
        //         var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
        //         var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        //         var peers = await peerService.GetPeers();
        //         if (peers.Count > 0)
        //         {
        //             var filter = new ActivityStreamFilter { sinceLastUpdated = LastTimeSyncedShops };
        //             foreach (var peer in peers)
        //             {
        //                 if (peer != null)
        //                 {
        //                     var client = await peerService.Open(peer.Address, peer.AddressPort);
        //                     // , await authService.GetKeyPair(peer.PublicKey)
        //                     try
        //                     {
        //                         var items = await client.GetShops(filter);
        //                         // await elastic.SyncShops(peer.PublicKey.Id, items);
        //                     }
        //                     catch (Exception e)
        //                     {
        //                         Console.WriteLine($"Error: {e.Message}");
        //                         Console.WriteLine(e);
        //                     }
        //                 }
        //             }
        //             LastTimeSyncedShops = DateTime.UtcNow;
        //         }
        //         else
        //         {
        //             Console.WriteLine("No peers");
        //         }

        //         var locals = await scope.ServiceProvider.GetRequiredService<IShopService>().GetAllLocal();
        //         await elastic.SyncShops(0, locals);
        //     }
        // }

        // public static async Task SyncInventories(ElasticRunner applicationServices)
        // {
        //     using (var scope = Services.CreateScope())
        //     {
        //         var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
        //         var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
        //         var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        //         var peers = await peerService.GetPeers();
        //         if (peers.Count > 0)
        //         {
        //             foreach (var peer in peers)
        //             {
        //                 if (peer != null)
        //                 {
        //                     var client = await peerService.Open(peer.Address, peer.AddressPort);
        //                     try
        //                     {
        //                         //var items = await client.GetInventories(LastTimeSyncedShops, await authService.GetKeyPair(peer.PublicKey));
        //                         //await elastic.SyncInventories(peer.PublicKey.Id, items);
        //                         //LastTimeSyncedShops = DateTime.UtcNow;
        //                     }
        //                     catch (Exception e)
        //                     {
        //                         Console.WriteLine($"Error: {e.Message}");
        //                         Console.WriteLine(e);
        //                     }
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             Console.WriteLine("No peers");
        //         }

        //         var locals = await scope.ServiceProvider.GetRequiredService<ISimpleInventoryService>().GetAllLocal();
        //         await elastic.SyncInventories(0, locals);
        //     }
        // }

        // public static void SyncReactions(ElasticRunner applicationServices)
        // {
        // }

        // public static void SyncMentions(ElasticRunner applicationServices)
        // {
        // }

        // public static async Task SyncPeers(ElasticRunner applicationServices)
        // {
        //     using (var scope = Services.CreateScope())
        //     {
        //         var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
        //         var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
        //         var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        //         var peers = await peerService.GetPeers();
        //         if (peers.Count > 0)
        //         {
        //             var filter = new ActivityStreamFilter { sinceLastUpdated = LastTimeSyncedPeers };
        //             foreach (var peer in peers)
        //             {
        //                 if (peer != null)
        //                 {
        //                     var client = await peerService.Open(peer.Address, peer.AddressPort);
        //                     // , await authService.GetKeyPair(peer.PublicKey)
        //                     try
        //                     {
        //                         var items = await client.GetPeers(filter);
        //                         //await elastic.SyncPeers(peer.PublicKey.Id, items);
        //                     }
        //                     catch (Exception e)
        //                     {
        //                         Console.WriteLine($"Error: {e.Message}");
        //                         Console.WriteLine(e);
        //                     }
        //                 }
        //             }
        //             LastTimeSyncedPeers = DateTime.UtcNow;
        //         }
        //         else
        //         {
        //             Console.WriteLine("No peers");
        //         }
        //     }
        // }
    }
}