using PinkUmbrella.Models.Search;
using Tides.Core;

namespace PinkUmbrella.ViewModels.Shared
{
    public class TagsViewModel : BaseViewModel
    {
        public CollectionObject Tags { get; set; }

        public SearchResultType? Type { get; set; }
    }
}