using System.Text.Json.Serialization;

namespace Tides.Models
{
    public class ErrorModel
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}