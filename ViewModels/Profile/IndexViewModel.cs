using seattle.Models;

namespace seattle.ViewModels.Profile
{
    public class IndexViewModel: BaseViewModel
    {
        public UserProfileModel Profile { get; set; }
        public FeedModel Feed { get; set; }
    }
}