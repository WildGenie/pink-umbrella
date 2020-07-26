using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.ViewModels.Shared
{
    public class ReactViewModel
    {
        public int? ViewerId { get; set; }
        public ReactionType Type { get; set; }
        public ReactionSubject Subject { get; set; }
        public PublicId ToId { get; set; }

        public bool HasReacted { get; set; }
        public int Count { get; set; }

        public bool Enabled { get; set; }

        public string ManualReactionUrl { get; set; }
        
        public string DataResponseOnClosest { get; set; }

        public string ResponseHandler { get; set; } = "replacewith";
    }
}