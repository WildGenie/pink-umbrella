using Estuary.Core;
using Estuary.Services.Boxes;

namespace Estuary.Services.BoxProviders
{
    public class SharedActivityStreamBoxProvider : IActivityStreamBoxProvider
    {
        public IActivityStreamBox Resolve(ActivityStreamFilter filter, IActivityStreamRepository ctx)
        {
            switch (filter.index)
            {
                case "sharedInbox":
                return new ActivityStreamSharedInbox(filter, ctx);
                case "sharedOutbox":
                return new ActivityStreamSharedOutbox(filter, ctx);
                case "globalInbox":
                return new ActivityStreamSharedInbox(filter, ctx);
                case "globalOutbox":
                return new ActivityStreamSharedOutbox(filter, ctx);
            }
            
            // if (filter.index.Contains('/'))
            // {
            //     return new ObjectActivityStream(filter, ctx);
            // }
            return null;
        }
    }
}