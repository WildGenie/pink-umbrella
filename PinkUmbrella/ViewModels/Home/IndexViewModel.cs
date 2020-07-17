using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Home
{
    public class IndexViewModel: BaseViewModel
    {
        public FeedModel MyFeed { get; set; }
        public FeedSource Source { get; set; }
        public NewPostViewModel NewPost { get; set; } = new NewPostViewModel();
    }
}