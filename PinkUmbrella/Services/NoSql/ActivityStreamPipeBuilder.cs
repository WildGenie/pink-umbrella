using System.Collections.Generic;
using Tides.Services;

namespace PinkUmbrella.Services.NoSql
{
    public class ActivityStreamPipeBuilder : IHazActivityStreamPipe
    {
        private readonly ActivityStreamPipe pipe;

        public ActivityStreamPipeBuilder(IEnumerable<IActivityStreamPipe> handlers)
        {
            this.pipe = new ActivityStreamPipe(handlers);
        }

        public ActivityStreamPipe Get() => pipe;
    }
}