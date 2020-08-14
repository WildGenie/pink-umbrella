using System;
using Elasticsearch.Net;
using Nest;
using Estuary.Core;
using Estuary.Actors;

namespace PinkUmbrella.Services.Elastic
{
    public abstract class BaseElasticService
    {
        protected ElasticClient GetClient()
        {
            var uri = new Uri("http://localhost:9200");
            var pool = new SingleNodeConnectionPool(uri);
            var settings = new ConnectionSettings(pool)
                .DefaultMappingFor<ActorObject>(m => m
                    .IndexName("actors")
                )
                .DefaultMappingFor<BaseObject>(m => m
                    .IndexName("objects")
                );
            return new ElasticClient(settings);
        }
    }
}