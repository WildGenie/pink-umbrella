using seattle.Models;

namespace seattle.Services.Sql
{
    public class GeoLocationService: IGeoLocationService
    {
        public GeoLocationModel Get(int id) {
            return new GeoLocationModel();
        }
        
        public void Add(GeoLocationModel location) {
            
        }

        public void Save(GeoLocationModel location) {
            
        }
    }
}