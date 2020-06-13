using System.Collections.Generic;

namespace seattle.Models
{
    public class FeedModel: PaginatedModel<PostModel>
    {
        public int UserId { get; set; }
        public int ViewerId { get; set; }
        public bool RepliesIncluded { get; set; }
    }
}