using System.Collections.Generic;
using Tides.Core;
using Tides.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class NewPostResult
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public List<BaseObject> Posts { get; set; }
    }
}