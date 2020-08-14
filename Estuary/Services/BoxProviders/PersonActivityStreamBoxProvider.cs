using Estuary.Core;
using Estuary.Services.Boxes;

namespace Estuary.Services.BoxProviders
{
    public class PersonActivityStreamBoxProvider : IActivityStreamBoxProvider
    {
        public IActivityStreamBox Resolve(ActivityStreamFilter filter, IActivityStreamRepository ctx)
        {
            if (filter.userId.HasValue)
            {
                switch (filter.index)
                {
                    case "notifications": // forward items from inbox to list that should be listened to based on settings
                    return new ActivityStreamInbox(filter, ctx);
                }
            }
            return null;
        }
    }
}