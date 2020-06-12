using System.Collections.Generic;
using seattle.Models;

namespace seattle.ViewModels.Inventory
{
    public class NewResourceViewModel
    {
        public SimpleResourceModel Resource { get; set; } = new SimpleResourceModel();
        public List<string> AvailableCategories { get; set; } = new List<string>();
        public List<string> AvailableBrands { get; set; } = new List<string>();
        public List<string> AvailableUnits { get; set; } = new List<string>();
    }
}