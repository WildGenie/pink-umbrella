using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PinkUmbrella.Util;
using Tides.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class SimpleResourceModel
    {
        public int Id { get; set; }

        [DisplayName("Inventory"), Description("Which inventory is this resource stored in?")]
        public int InventoryId { get; set; }

        [DisplayName("Forked From")]
        public int ForkedFromId { get; set; }

        [DisplayName("Created By")]
        public int CreatedByUserId { get; set; }


        [Required, StringLength(100), Description("What type of resource is this?"), InputPlaceHolder("e.g. Medical, food, water, restroom"), DebugValue("Medical")]
        public string Category { get; set; }

        [Required, StringLength(100), Description("What is the resource called?"), InputPlaceHolder("e.g. band-aids, water bottles, paper and markers"), DebugValue("Band-Aids")]
        public string Name { get; set; }

        [Required, StringLength(100), Description("What is the resource called?"), InputPlaceHolder("e.g. generic, Name-Brand"), DebugValue("Generic")]
        public string Brand { get; set; }

        [Required, StringLength(1000), Description("What makes this resource special or unique?"), InputPlaceHolder("e.g. these band-aids are for small to medium wounds."), DebugValue("These band-aids are for small to medium wounds.")]
        public string Description { get; set; }

        [Required, StringLength(100), Description("How do you measure this resource? Leave blank for no units."), InputPlaceHolder("e.g. cups, pounds, dozens")]
        public string Units { get; set; }

        [DefaultValue(1), Description("How much / many of this resource does the inventory have?")]
        public double Amount { get; set; }


        public SimpleResourceModel() {

        }

        public SimpleResourceModel(SimpleResourceModel other) {
            Id = other.Id;
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