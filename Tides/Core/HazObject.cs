using System.Text.Json.Serialization;

namespace Tides.Core
{
    public class HazObject : BaseObject
    {
        public HazObject(string type = null) : base(type)
        {
        }

        [JsonPropertyName("object")]
        public BaseObject obj { get; set; }
    }
}