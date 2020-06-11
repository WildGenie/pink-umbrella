using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace seattle.Models
{
    public class PostTagModel
    {
        [JsonIgnore]
        public int Id { get; set; }

        [StringLength(100)]
        public string Text { get; set; }
    }
}