namespace seattle.Models
{
    public class SimpleResourceModel
    {
        public int id { get; set; }
        public int mipmap_id { get; set; }
        public int inventory_id { get; set; }
        public int forked_from_id { get; set; }
        public int created_by_user_id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public string Units { get; set; }
        public double Amount { get; set; }
    }
}