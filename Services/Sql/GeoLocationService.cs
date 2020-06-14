using PinkUmbrella.Models;

namespace PinkUmbrella.Services.Sql
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