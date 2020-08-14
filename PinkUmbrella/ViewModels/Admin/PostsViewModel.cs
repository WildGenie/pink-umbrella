using Estuary.Core;

namespace PinkUmbrella.ViewModels.Admin
{
    public class PostsViewModel : BaseViewModel
    {
        public OrderedCollectionPageObject MostReportedPosts { get; set; }
        
        public OrderedCollectionPageObject MostBlockedPosts { get; set; }

        public OrderedCollectionPageObject MostDislikedPosts { get; set; }
    }
}