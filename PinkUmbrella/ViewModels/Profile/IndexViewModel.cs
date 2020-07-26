using PinkUmbrella.Models.Community;
using Tides.Core;

namespace PinkUmbrella.ViewModels.Profile
{
    public class IndexViewModel: ProfileViewModel
    {
        public CollectionObject Feed { get; set; }

        public CollectionObject Media { get; set; }

        public CollectionObject Shops { get; set; }
        
        public CollectionObject Users { get; set; }
        
        public UserListType UserListType { get; set; }
    }
}