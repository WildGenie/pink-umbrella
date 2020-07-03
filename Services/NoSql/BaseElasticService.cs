using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Hangfire;
using Nest;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Elastic;

namespace PinkUmbrella.Services.NoSql
{
    public class ElasticService: IElasticService
    {
        public async Task SyncProfiles(long authId, List<ElasticProfileModel> profiles)
        {
            if (profiles == null || profiles.Count == 0)
            {
                Console.WriteLine($"No profiles to sync for {authId}");
                return;
            }

            var client = GetClient();

            foreach (var profile in profiles)
            {
                var result = await client.IndexAsync(profile, i => i.Index("profiles"));
                if (result.Result == Result.Created || result.Result == Result.Updated)
                {
                    Console.WriteLine($"{result.Result} {profile.Id}");
                }
                else
                {
                    Console.WriteLine($"Error adding {profile.Id}: {result}");
                }
            }
        }

        protected ElasticClient GetClient()
        {
            var uri = new Uri("http://localhost:9200");
            var pool = new SingleNodeConnectionPool(uri);
            return new ElasticClient(new ConnectionSettings(pool));
        }

        protected async Task TestAsync()
        {   
            var client = GetClient();
            var result = await client.IndexDocumentAsync(new DocumentPath<ElasticProfileModel>(new Id("1")));
        }
    }
}