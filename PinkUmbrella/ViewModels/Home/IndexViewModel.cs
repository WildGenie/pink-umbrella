using PinkUmbrella.ViewModels.Shared;

namespace PinkUmbrella.ViewModels.Home
{
    public class IndexViewModel: BaseViewModel
    {
        public ListViewModel MyFeed { get; set; }
        public NewPostViewModel NewPost { get; set; } = new NewPostViewModel();
    }
}