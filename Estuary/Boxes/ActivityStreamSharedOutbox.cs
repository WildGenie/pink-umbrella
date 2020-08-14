using Estuary.Core;


namespace Estuary.Services.Boxes
{
    public class ActivityStreamSharedOutbox : BaseActivityStreamBox
    {
        public ActivityStreamSharedOutbox(ActivityStreamFilter filter, IActivityStreamRepository ctx) : base(filter, ctx)
        {
        }
    }
}