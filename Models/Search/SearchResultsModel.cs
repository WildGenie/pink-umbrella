using System.Collections.Generic;

namespace PinkUmbrella.Models.Search
{
    public class SearchResultsModel
    {
        public List<SearchResultModel> Results { get; set; } = new List<SearchResultModel>();
        
        public int TotalResults { get; set; } = -1;
    }
}