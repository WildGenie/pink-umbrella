using System.Text.Json.Serialization;

namespace PinkUmbrella.Models.Elastic
{
    public class ElasticProfileModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("authId")]
        public long AuthId { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("handle")]
        public string Handle { get; set; }
        
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }


        public void SetIdFromCompound()
        {
            Id = $"{AuthId}-{UserId}";
        }

        public void SetCompoundIdFromId()
        {
            var split = Id.Split('-');
            AuthId = long.Parse(split[0]);
            UserId = int.Parse(split[1]);
        }
    }
}