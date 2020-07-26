namespace PinkUmbrella.Models
{
    public class ObjectShadowBanModel
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public int ObjectId { get; set; }
        public long PeerId { get; set; }
    }
}