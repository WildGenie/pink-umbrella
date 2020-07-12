using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using PinkUmbrella.Util;

namespace PinkUmbrella.Services.Jobs
{
    public static class ElasticJobs
    {
        public static IServiceProvider Services { get; set; }

        public static DateTime? LastTimeSyncedProfiles { get; set; }

        public static DateTime? LastTimeSyncedPosts { get; set; }

        public static DateTime? LastTimeSyncedShops { get; set; }

        public static DateTime? LastTimeSyncedArchivedMedia { get; set; }

        public static DateTime? LastTimeSyncedPeers { get; set; }


        [AutomaticRetry(Attempts = 0)]
        public static async Task SyncProfiles(IServiceProvider s)
        {
            using (var scope = Services.CreateScope())
            {
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
                var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
                var peers = await peerService.GetPeers();
                if (peers.Count > 0)
                {
                    foreach (var peer in peers)
                    {
                        if (peer != null)
                        {
                            var client = await peerService.Open(peer.Address, peer.AddressPort);
                            try
                            {
                                var profiles = await client.GetProfiles(LastTimeSyncedProfiles);
                                await elastic.SyncProfiles(peer.PublicKey.Id, profiles);
                                LastTimeSyncedProfiles = DateTime.UtcNow;
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

                var locals = await scope.ServiceProvider.GetRequiredService<IPublicProfileService>().GetAllLocal();
                await elastic.SyncProfiles(0, locals);
            }
        }

        public static async Task SyncPosts(ElasticRunner applicationServices)
        {
            using (var scope = Services.CreateScope())
            {
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
                var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
                var peers = await peerService.GetPeers();
                if (peers.Count > 0)
                {
                    foreach (var peer in peers)
                    {
                        if (peer != null)
                        {
                            var client = await peerService.Open(peer.Address, peer.AddressPort);
                            try
                            {
                                var items = await client.GetPosts(LastTimeSyncedPosts);
                                await elastic.SyncPosts(peer.PublicKey.Id, items);
                                LastTimeSyncedPosts = DateTime.UtcNow;
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

                var locals = await scope.ServiceProvider.GetRequiredService<IPostService>().GetAllLocal();
                await elastic.SyncPosts(0, locals);
            }
        }

        public static async Task SyncMedia(ElasticRunner applicationServices)
        {
            using (var scope = Services.CreateScope())
            {
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
                var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
                var peers = await peerService.GetPeers();
                if (peers.Count > 0)
                {
                    foreach (var peer in peers)
                    {
                        if (peer != null)
                        {
                            var client = await peerService.Open(peer.Address, peer.AddressPort);
                            try
                            {
                                var medias = await client.GetArchivedMedia(LastTimeSyncedArchivedMedia);
                                await elastic.SyncArchivedMedias(peer.PublicKey.Id, medias);
                                LastTimeSyncedArchivedMedia = DateTime.UtcNow;
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

                var locals = await scope.ServiceProvider.GetRequiredService<IArchiveService>().GetAllLocal();
                await elastic.SyncArchivedMedias(0, locals);
            }
        }

        public static async Task SyncShops(ElasticRunner applicationServices)
        {
            using (var scope = Services.CreateScope())
            {
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
                var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
                var peers = await peerService.GetPeers();
                if (peers.Count > 0)
                {
                    foreach (var peer in peers)
                    {
                        if (peer != null)
                        {
                            var client = await peerService.Open(peer.Address, peer.AddressPort);
                            try
                            {
                                var items = await client.GetShops(LastTimeSyncedShops);
                                await elastic.SyncShops(peer.PublicKey.Id, items);
                                LastTimeSyncedShops = DateTime.UtcNow;
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

                var locals = await scope.ServiceProvider.GetRequiredService<IShopService>().GetAllLocal();
                await elastic.SyncShops(0, locals);
            }
        }

        public static async Task SyncInventories(ElasticRunner applicationServices)
        {
            using (var scope = Services.CreateScope())
            {
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
                var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
                var peers = await peerService.GetPeers();
                if (peers.Count > 0)
                {
                    foreach (var peer in peers)
                    {
                        if (peer != null)
                        {
                            var client = await peerService.Open(peer.Address, peer.AddressPort);
                            try
                            {
                                var items = await client.GetInventories(LastTimeSyncedShops);
                                await elastic.SyncInventories(peer.PublicKey.Id, items);
                                LastTimeSyncedShops = DateTime.UtcNow;
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

                var locals = await scope.ServiceProvider.GetRequiredService<ISimpleInventoryService>().GetAllLocal();
                await elastic.SyncInventories(0, locals);
            }
        }

        public static void SyncReactions(ElasticRunner applicationServices)
        {
        }

        public static void SyncMentions(ElasticRunner applicationServices)
        {
        }

        public static async Task SyncPeers(ElasticRunner applicationServices)
        {
            using (var scope = Services.CreateScope())
            {
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();
                var peerService = scope.ServiceProvider.GetRequiredService<IPeerService>();
                var peers = await peerService.GetPeers();
                if (peers.Count > 0)
                {
                    foreach (var peer in peers)
                    {
                        if (peer != null)
                        {
                            var client = await peerService.Open(peer.Address, peer.AddressPort);
                            try
                            {
                                var items = await client.GetPeers(LastTimeSyncedPeers);
                                await elastic.SyncPeers(peer.PublicKey.Id, items);
                                LastTimeSyncedPeers = DateTime.UtcNow;
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
            }
        }
    }
}