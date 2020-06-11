using seattle.Models;

namespace seattle.ViewModels.Home
{
    public class IndexViewModel
    {
        public UserProfileModel MyProfile { get; set; }
        public FeedModel MyFeed { get; set; }
    }
}