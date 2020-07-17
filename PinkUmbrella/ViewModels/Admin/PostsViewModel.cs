using System.Collections.Generic;
using PinkUmbrella.Models;
using Poncho.Models;

namespace PinkUmbrella.ViewModels.Admin
{
    public class PostsViewModel : BaseViewModel
    {
        public PaginatedModel<PostModel> MostReportedPosts { get; set; }
        
        public PaginatedModel<PostModel> MostBlockedPosts { get; set; }

        public PaginatedModel<PostModel> MostDislikedPosts { get; set; }
    }
}