using System;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using PinkUmbrella.Util;

namespace PinkUmbrella.Services.Jobs
{
    public static class ElasticJobs
    {
        public static IServiceProvider Services { get; set; }

        public static DateTime? LastTimeSyncedProfiles { get; set; }


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
                            await elastic.SyncProfiles(peer.PublicKey.Id, await client.GetProfiles(LastTimeSyncedProfiles));
                            LastTimeSyncedProfiles = DateTime.UtcNow;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No peers");
                }
            }
        }

        public static void SyncPosts(ElasticRunner applicationServices)
        {
        }

        public static void SyncMedia(ElasticRunner applicationServices)
        {
        }

        public static void SyncShops(ElasticRunner applicationServices)
        {
        }

        public static void SyncReactions(ElasticRunner applicationServices)
        {
        }

        public static void SyncMentions(ElasticRunner applicationServices)
        {
        }

        public static void SyncPeers(ElasticRunner applicationServices)
        {
        }
    }
}