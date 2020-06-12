using System.Collections.Generic;
using seattle.Models;

namespace seattle.ViewModels.Post
{
    public class PostViewModel
    {
        public UserProfileModel MyProfile { get; set; }
        public PostModel Post { get; set; }
        public List<PostModel> Chain { get; set; }
    }
}