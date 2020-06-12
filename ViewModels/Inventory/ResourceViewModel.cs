using seattle.Models;

namespace seattle.ViewModels.Inventory
{
    public class ResourceViewModel
    {
        public SimpleResourceModel Resource { get; set; }
        public UserProfileModel MyProfile { get; set; }
        public string ReturnUrl { get; set; }
    }
}