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


        public SimpleResourceModel() {

        }

        public SimpleResourceModel(SimpleResourceModel other) {
            Id = other.Id;
            WhenCreated = other.WhenCreated;
            WhenDeleted = other.WhenDeleted;
            DeletedByUserId = other.DeletedByUserId;
            MipmapId = other.MipmapId;
            ForkedFromId = other.ForkedFromId;
            CreatedByUserId = other.CreatedByUserId;
            Category = other.Category;
            Name = other.Name;
            Brand = other.Brand;
            Description = other.Description;
            Units = other.Units;
            Amount = other.Amount;
        }
    }
}