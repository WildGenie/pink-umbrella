using System.Collections.Generic;

namespace PinkUmbrella.Models
{
    public class NewPostResult
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public List<PostModel> Posts { get; set; }
    }
}