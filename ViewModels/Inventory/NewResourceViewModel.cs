using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Inventory
{
    public class NewResourceViewModel: BaseViewModel
    {
        public SimpleResourceModel Resource { get; set; } = new SimpleResourceModel();
        public List<string> AvailableCategories { get; set; } = new List<string>();
        public List<string> AvailableBrands { get; set; } = new List<string>();
        public List<string> AvailableUnits { get; set; } = new List<string>();
    }
}