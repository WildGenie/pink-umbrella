using System;

namespace PinkUmbrella.Util
{
    public class ElasticRunner
    {
        public IServiceProvider services;

        public ElasticRunner(IServiceProvider services)
        {
            this.services = services;
        }
    }
}