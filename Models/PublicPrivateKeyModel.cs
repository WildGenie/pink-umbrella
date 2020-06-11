namespace seattle.Models
{
    public class PublicPrivateKeyModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string KeyType { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}