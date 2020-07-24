using System;
using Elasticsearch.Net;
using Nest;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Peer;
using PinkUmbrella.Models.Public;
using Tides.Models;
using Tides.Models.Peer;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Elastic
{
    public abstract class BaseElasticService
    {
        protected ElasticClient GetClient()
        {
            var uri = new Uri("http://localhost:9200");
            var pool = new SingleNodeConnectionPool(uri);
            var settings = new ConnectionSettings(pool)
                .DefaultMappingFor<PublicProfileModel>(m => m
                    .IndexName("profiles")
                )
                .DefaultMappingFor<ShopModel>(m => m
                    .IndexName("shops")
                )
                .DefaultMappingFor<PostModel>(m => m
                    .IndexName("posts")
                )
                .DefaultMappingFor<ArchivedMediaModel>(m => m
                    .IndexName("archived-media")
                )
                .DefaultMappingFor<SimpleInventoryModel>(m => m
                    .IndexName("inventories")
                )
                .DefaultMappingFor<PeerModel>(m => m
                    .IndexName("peers")
                );
            return new ElasticClient(settings);
        }
    }
}