using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace seattle.Models
{
    public class SimpleResourceModel
    {
        public int Id { get; set; }
        public DateTime WhenCreated { get; set; }

        [DefaultValue(null)]
        public DateTime? WhenDeleted { get; set; }

        [DefaultValue(null)]
        public int? DeletedByUserId { get; set; }

        public int MipmapId { get; set; }
        public int InventoryId { get; set; }
        public int ForkedFromId { get; set; }
        public int CreatedByUserId { get; set; }


        [Required, StringLength(100)]
        public string Category { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(100)]
        public string Brand { get; set; }

        [Required, StringLength(1000)]
        public string Description { get; set; }

        [Required, StringLength(100)]
        public string Units { get; set; }

        [DefaultValue(1)]
        public double Amount { get; set; }
    }
}