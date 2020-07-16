using System.Collections.Generic;
using PinkUmbrella.Util;

namespace PinkUmbrella.Models.Search
{
    [IsDocumented]
    public class SearchResultsModel
    {
        public List<SearchResultModel> Results { get; set; } = new List<SearchResultModel>();
        
        public int TotalResults { get; set; } = -1;
    }
}