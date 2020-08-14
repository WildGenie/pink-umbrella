using Estuary.Core;
using Estuary.Util;

namespace Estuary.Services.Boxes
{
    public class ActivityStreamOutbox : BaseActivityStreamBox
    {
        public ActivityStreamOutbox(ActivityStreamFilter filter, IActivityStreamRepository ctx) : base(filter, ctx)
        {
        }
    }
}