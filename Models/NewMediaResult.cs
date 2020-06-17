using System.Collections.Generic;

namespace PinkUmbrella.Models
{
    public class NewMediaResult
    {
        public bool Error { get; internal set; }
        public List<ArchivedMediaModel> Medias { get; internal set; }
    }
}