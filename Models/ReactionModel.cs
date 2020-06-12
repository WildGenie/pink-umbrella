using System;

namespace seattle.Models
{
    public class ReactionModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ToId { get; set; }
        public DateTime WhenReacted { get; set; }
        public ReactionType Type { get; set; }
    }
}