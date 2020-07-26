namespace Tides.Core
{
    public class ActivityObject : HazObject
    {
        public ActivityObject(string type = null) : base(type ?? "Activity")
        {
        }

        public CollectionObject actor { get; set; }

        public CollectionObject target { get; set; }

        public BaseObject result { get; set; }

        public BaseObject origin { get; set; }

        public CollectionObject instrument { get; set; }
    }
}