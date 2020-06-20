using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Profile
{
    public class IndexViewModel: ProfileViewModel
    {
        public FeedModel Feed { get; set; }

        public PaginatedModel<ArchivedMediaModel> Media { get; set; }

        public List<ShopModel> Shops { get; set; }
        
        public List<UserProfileModel> Users { get; set; }
        
        public UserListType UserListType { get; set; }
    }
}