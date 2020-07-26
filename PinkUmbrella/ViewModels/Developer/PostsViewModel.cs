using Tides.Core;

namespace PinkUmbrella.ViewModels.Developer
{
    public class PostsViewModel : BaseViewModel
    {
        public CollectionObject MostReportedPosts { get; set; }
        
        public CollectionObject MostBlockedPosts { get; set; }

        public CollectionObject MostDislikedPosts { get; set; }
    }
}