using System.Collections.Generic;

namespace seattle.Models
{
    public class FeedModel
    {
        public int UserId { get; set; }
        public int ViewerId { get; set; }
        public bool RepliesIncluded { get; set; }
        public int TotalPosts { get; set; }
        public List<PostModel> Posts { get; set; }
        public PaginationModel Pagination { get; set; }
    }
}