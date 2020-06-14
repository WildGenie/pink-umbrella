using seattle.Models;

namespace seattle.ViewModels.Shared
{
    public class ReactViewModel
    {
        public int? ViewerId { get; set; }
        public ReactionType Type { get; set; }
        public ReactionSubject Subject { get; set; }
        public int ToId { get; set; }

        public bool HasReacted { get; set; }
        public int Count { get; set; }

        public bool Enabled { get; set; }
    }
}