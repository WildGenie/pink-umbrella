using System.Collections.Generic;
using System.ComponentModel;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Inventory
{
    public class IndexViewModel
    {
        [DisplayName("Inventory")]
        public int? InventoryId { get; set; }
        public int? SelectedId { get; set; }
        public UserProfileModel MyProfile { get; set; }
        public List<SimpleResourceModel> Resources { get; set; } = new List<SimpleResourceModel>();
        public List<SimpleInventoryModel> Inventories { get; set; } = new List<SimpleInventoryModel>();
        public SimpleInventoryModel Inventory { get; set; }
        public NewResourceViewModel NewResource { get; set; } = new NewResourceViewModel();
        public SimpleInventoryModel NewInventory { get; set; } = new SimpleInventoryModel();

        public bool AddResourceEnabled { get; set; } = false;
    }
}