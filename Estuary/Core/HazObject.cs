using System.Text.Json.Serialization;

namespace Estuary.Core
{
    public class HazObject : BaseObject
    {
        public HazObject(string type, string baseType) : base(type, baseType)
        {
        }

        [JsonPropertyName("object")]
        public BaseObject obj { get; set; }
    }
}