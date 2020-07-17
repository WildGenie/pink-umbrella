using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Inventory
{
    public class ResourceViewModel: BaseViewModel
    {
        public SimpleResourceModel Resource { get; set; }
        public string ReturnUrl { get; set; }
    }
}