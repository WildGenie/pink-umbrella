using System.Collections.Generic;

namespace Tides.Core
{
    public class ActivityObject : HazObject
    {
        public ActivityObject(string type = null) : base(type ?? "Activity")
        {
        }

        public List<BaseObject> actor { get; set; }

        public List<BaseObject> target { get; set; }

        public BaseObject result { get; set; }

        public BaseObject origin { get; set; }

        public List<BaseObject> instrument { get; set; }
    }
}