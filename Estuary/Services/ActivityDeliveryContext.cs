using System.Collections.Generic;
using Estuary.Core;

namespace Estuary.Services
{
    public class ActivityDeliveryContext
    {
        public ActivityObject item { get; set; }
        public IActivityStreamBox box { get; set; }
        public IActivityStreamRepository context { get; set; }
        public bool IsReading { get; set; }
        public bool IsWriting { get; set; }
        public bool HasWritten { get; set; }
        public ActivityStreamFilter Filter { get; set; }
        public List<string> Undos { get; set; } = new List<string>();

        // public void Forward(ActivityObject item, IActivityStreamBox box)
        // {
        //     context.Post(new ActivityDeliveryContext { item = item, context = context, box = box });
        // }
    }
}