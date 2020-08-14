using Estuary.Core;

namespace Estuary.Services.Boxes
{
    public class ActivityStreamInbox : BaseActivityStreamBox
    {
        public ActivityStreamInbox(ActivityStreamFilter filter, IActivityStreamRepository ctx) : base(filter, ctx)
        {
        }
    }
}