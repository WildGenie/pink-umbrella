using System.Text.Json.Serialization;

namespace Poncho.Models
{
    public class ErrorModel
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}