using System.Collections.Generic;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Search;

namespace PinkUmbrella.ViewModels.Shared
{
    public class TagsViewModel : BaseViewModel
    {
        public List<TagModel> Tags { get; set; }

        public SearchResultType? Type { get; set; }
    }
}