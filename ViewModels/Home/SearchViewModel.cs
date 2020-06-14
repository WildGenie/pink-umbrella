using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Home
{
    public class SearchViewModel
    {
        public string SearchText { get; set; }
        public SearchResultType? Type { get; set; }
        public SearchResultOrder Order { get; set; }
        public SearchResultsModel Results { get; set; }
        public PaginationModel Pagination { get; set; }
        public object MyProfile { get; internal set; }
    }
}