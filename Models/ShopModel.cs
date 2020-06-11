namespace seattle.Models
{
    public class ShopModel
    {
        public int id { get; set; }
        public string handle { get; set; }
        public string Name { get; set; }
        public int GeoLocationId { get; set; }

        
        public GeoLocationModel GeoLocation { get; set; }
    }
}