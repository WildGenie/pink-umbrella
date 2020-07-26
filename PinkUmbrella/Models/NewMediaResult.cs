using System.Collections.Generic;
using Tides.Core;
using Tides.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class NewMediaResult
    {
        public bool Error { get; internal set; }
        public List<BaseObject> Medias { get; internal set; }
    }
}