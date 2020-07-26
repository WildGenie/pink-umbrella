using System.Collections.Generic;
using System.ComponentModel;
using Tides.Core;

namespace PinkUmbrella.ViewModels.Inventory
{
    public class IndexViewModel: BaseViewModel
    {
        [DisplayName("Inventory")]
        public string InventoryId { get; set; }
        public string SelectedId { get; set; }
        public CollectionObject Resources { get; set; } = new CollectionObject();
        public CollectionObject Inventories { get; set; } = new CollectionObject();
        public BaseObject Inventory { get; set; }
        public NewResourceViewModel NewResource { get; set; } = new NewResourceViewModel();
        public NewInventoryViewModel NewInventory { get; set; } = null;

        public bool AddResourceEnabled { get; set; } = false;
    }
}