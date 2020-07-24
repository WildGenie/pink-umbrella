using System.Collections.Generic;
using PinkUmbrella.Models;
using Tides.Models;

namespace PinkUmbrella.ViewModels.Developer
{
    public class PostsViewModel : BaseViewModel
    {
        public PaginatedModel<PostModel> MostReportedPosts { get; set; }
        
        public PaginatedModel<PostModel> MostBlockedPosts { get; set; }

        public PaginatedModel<PostModel> MostDislikedPosts { get; set; }
    }
}