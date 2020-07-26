using PinkUmbrella.Models;
using Tides.Core;

namespace PinkUmbrella.ViewModels.Home
{
    public class IndexViewModel: BaseViewModel
    {
        public CollectionObject MyFeed { get; set; }
        public FeedSource Source { get; set; }
        public NewPostViewModel NewPost { get; set; } = new NewPostViewModel();
    }
}