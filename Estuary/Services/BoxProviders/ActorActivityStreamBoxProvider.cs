using Estuary.Core;
using Estuary.Services.Boxes;
using Estuary.Util;

namespace Estuary.Services.BoxProviders
{
    public class ActorActivityStreamBoxProvider : IActivityStreamBoxProvider
    {
        public IActivityStreamBox Resolve(ActivityStreamFilter filter, IActivityStreamRepository ctx)
        {
            if (filter.id.Id.HasValue && filter.id.PeerId.HasValue)
            {
                switch (filter.index)
                {
                    case "inbox":
                    return new ActivityStreamInbox(filter, ctx);
                    case "outbox":
                    {
                        var ret = new ActivityStreamOutbox(filter, ctx);
                        if (filter.types == null)
                        {
                            foreach (var activityToIndexOn in new string[] { "Create", "Announce", "Like", "Dislike" })
                            {
                                ret.CreateIndexOn(new ActivityStreamFilter(filter.index)
                                {
                                    types = new string[] { activityToIndexOn }
                                });
                            }
                        }
                        return ret;
                    }
                    case "shares":
                    return new ActivityStreamOutbox(filter.FixType("Share"), ctx);
                    case "liked":
                    return new ActivityStreamOutbox(filter.FixType("Like"), ctx);
                    case "disliked":
                    return new ActivityStreamOutbox(filter.FixType("Dislike"), ctx);
                    case "blocked":
                    return new ActivityStreamOutbox(filter.FixType("Block"), ctx);
                    case "reported":
                    return new ActivityStreamOutbox(filter.FixType("Report"), ctx);

                    case "followers":
                    return new BaseActivityStreamBox(filter.FixType("Add", "Remove").FixObjType("Actor"), ctx);
                    case "following":
                    return new BaseActivityStreamBox(filter.FixType("Add", "Remove").FixObjType("Actor"), ctx);
                }
            }
            return null;
        }
    }
}