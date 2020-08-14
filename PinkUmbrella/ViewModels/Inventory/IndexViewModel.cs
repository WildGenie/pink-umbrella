using System.ComponentModel;
using Estuary.Core;
using PinkUmbrella.ViewModels.Shared;

namespace PinkUmbrella.ViewModels.Inventory
{
    public class IndexViewModel: BaseViewModel
    {
        [DisplayName("Inventory")]
        public string InventoryId { get; set; }
        public ListViewModel Resources { get; set; } = null;
        public ListViewModel Inventories { get; set; } = null;
        public BaseObject Inventory { get; set; }
        public NewResourceViewModel NewResource { get; set; } = new NewResourceViewModel();
        public NewInventoryViewModel NewInventory { get; set; } = null;

        public bool AddResourceEnabled { get; set; } = false;
    }
}