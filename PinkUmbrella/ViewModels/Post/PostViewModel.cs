using System.Collections.Generic;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;

namespace PinkUmbrella.ViewModels.Post
{
    public class PostViewModel: BaseViewModel
    {
        public PostModel Post { get; set; }
        public List<PostModel> Chain { get; set; }
    }
}