using PinkUmbrella.Models.Public;

namespace PinkUmbrella.Models
{
    public class FeedModel: PaginatedModel<PostModel>
    {
        public PublicId UserId { get; set; }
        public int? ViewerId { get; set; }
        public bool RepliesIncluded { get; set; }
    }
}