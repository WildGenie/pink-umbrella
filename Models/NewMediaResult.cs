using System.Collections.Generic;
using PinkUmbrella.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class NewMediaResult
    {
        public bool Error { get; internal set; }
        public List<ArchivedMediaModel> Medias { get; internal set; }
    }
}