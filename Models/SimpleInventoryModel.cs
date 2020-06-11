namespace seattle.Models
{
    public class SimpleInventoryModel
    {
        public int Id { get; set; }
        public int OwnerUserId { get; set; }
        public int GeoLocationId { get; set; }

        
        public GeoLocationModel GeoLocation { get; set; }
        
    }
}