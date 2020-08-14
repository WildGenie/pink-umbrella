using System.Text.Json.Serialization;

namespace Estuary.Core
{
    public class ActivityObject : HazObject
    {
        public ActivityObject(string type, string baseType) : base(type ?? "Activity", baseType)
        {
        }

        public ActivityObject() : base("Activity", null)
        {
        }

        public CollectionObject actor { get; set; }

        public CollectionObject target { get; set; }

        public BaseObject result { get; set; }

        public BaseObject origin { get; set; }

        public CollectionObject instrument { get; set; }


        [JsonIgnore]
        public override int? UserId => base.UserId ?? actor?.PublicId?.Id;
    }
}