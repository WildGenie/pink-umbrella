using System.Collections.Generic;
using Poncho.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class NewPostResult
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public List<PostModel> Posts { get; set; }
    }
}