using System.Collections.Generic;
using seattle.Models;

namespace seattle.ViewModels.Inventory
{
    public class IndexViewModel
    {
        public UserProfileModel MyProfile { get; set; }
        public List<SimpleResourceModel> Resources { get; set; }
        public List<SimpleInventoryModel> Inventories { get; set; }
    }
}