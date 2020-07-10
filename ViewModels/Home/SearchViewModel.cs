using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;

namespace PinkUmbrella.ViewModels.Home
{
    public class SearchViewModel: BaseViewModel
    {
        public string SearchText { get; set; }
        public SearchResultType? Type { get; set; }
        public SearchResultOrder Order { get; set; }
        public SearchResultsModel Results { get; set; }
        public PaginationModel Pagination { get; set; }
    }
}