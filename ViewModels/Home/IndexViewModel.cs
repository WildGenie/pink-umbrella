using seattle.Models;

namespace seattle.ViewModels.Home
{
    public class IndexViewModel: BaseViewModel
    {
        public FeedModel MyFeed { get; set; }
        public FeedSource Source { get; set; }
    }
}