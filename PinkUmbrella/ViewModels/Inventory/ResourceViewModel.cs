using Estuary.Core;

namespace PinkUmbrella.ViewModels.Inventory
{
    public class ResourceViewModel: BaseViewModel
    {
        public BaseObject Resource { get; set; }
        public string ReturnUrl { get; set; }
    }
}