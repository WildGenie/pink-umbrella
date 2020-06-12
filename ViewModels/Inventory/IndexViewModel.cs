using System.Collections.Generic;
using seattle.Models;

namespace seattle.ViewModels.Inventory
{
    public class IndexViewModel
    {
        public int InventoryId { get; set; }
        public int SelectedId { get; set; }
        public UserProfileModel MyProfile { get; set; }
        public List<SimpleResourceModel> Resources { get; set; } = new List<SimpleResourceModel>();
        public List<SimpleInventoryModel> Inventories { get; set; } = new List<SimpleInventoryModel>();
        public SimpleInventoryModel Inventory { get; set; }
        public NewResourceViewModel NewResource { get; set; } = new NewResourceViewModel();
        public NewInventoryViewModel NewInventory { get; set; } = new NewInventoryViewModel();
    }
}