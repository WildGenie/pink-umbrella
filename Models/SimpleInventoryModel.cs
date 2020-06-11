namespace seattle.Models
{
    public class SimpleInventoryModel
    {
        public int Id { get; set; }
        public int OwnerUserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public GeoLocationModel GeoLocation { get; set; }
    }
}