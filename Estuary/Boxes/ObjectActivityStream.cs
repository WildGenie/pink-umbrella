using Estuary.Core;

namespace Estuary.Services.Boxes
{
    public class ObjectActivityStream : BaseActivityStreamBox
    {
        public ObjectActivityStream(ActivityStreamFilter filter, IActivityStreamRepository ctx) : base(filter, ctx)
        {
        }
    }
}