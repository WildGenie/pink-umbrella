using System.Text.Json.Serialization;
using Tides.Models.Public;

namespace Tides.Core
{
    public class BaseObjectReference: IHazPublicId
    {
        public string type { get; set; }

        public long hash { get; set; }

        [JsonPropertyName("peerId")]
        public long PeerId { get; set; }

        public int? objectId { get; set; }
        
        [JsonIgnore]
        public PublicId PublicId => objectId.HasValue ? new PublicId(objectId.Value, PeerId) : null;
    }
}