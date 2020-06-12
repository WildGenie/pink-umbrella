using System.Collections.Generic;

namespace seattle.Models
{
    public class SearchResultsModel
    {
        public List<SearchResultModel> Results { get; set; }
        public int TotalResults { get; set; }
    }
}