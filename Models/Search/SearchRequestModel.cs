using PinkUmbrella.Util;

namespace PinkUmbrella.Models.Search
{
    [IsDocumented]
    public class SearchRequestModel
    {
        public string text { get; set; }
        public int? viewerId { get; set; }
        public SearchResultType? type { get; set; }
        public SearchResultOrder order { get; set; }
        public PaginationModel pagination { get; set; }
        public string[] tags { get; set; }
    }
}