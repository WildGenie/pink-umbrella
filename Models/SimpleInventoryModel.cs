using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkUmbrella.Models
{
    public class SimpleInventoryModel
    {
        public int Id { get; set; }
        public int OwnerUserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required, StringLength(100), DisplayName("Display Name")]
        public string DisplayName { get; set; }

        [Required, StringLength(1000)]
        public string Description { get; set; }

        public DateTime WhenCreated { get; set; }
        
        [NotMapped]
        public GeoLocationModel GeoLocation { get; set; }
    }
}