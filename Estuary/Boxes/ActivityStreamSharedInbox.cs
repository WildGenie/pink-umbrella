using Estuary.Core;

namespace Estuary.Services.Boxes
{
    public class ActivityStreamSharedInbox : BaseActivityStreamBox
    {
        public ActivityStreamSharedInbox(ActivityStreamFilter filter, IActivityStreamRepository ctx) : base(filter, ctx)
        {
        }
    }
}