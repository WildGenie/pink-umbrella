using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace seattle.Models
{
    public class ShopModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [DefaultValue(Visibility.VISIBLE_TO_WORLD)]
        public Visibility Visibility { get; set; }

        [Required, StringLength(100)]
        public string Handle { get; set; }

        [Required, StringLength(200)]
        public string DisplayName { get; set; }

        public int GeoLocationId { get; set; }
        [NotMapped]
        public GeoLocationModel GeoLocation { get; set; }
    }
}