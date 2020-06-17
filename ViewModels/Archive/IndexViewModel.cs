using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Archive
{
    public class IndexViewModel : BaseViewModel
    {
        public PaginatedModel<ArchivedMediaModel> Items { get; set; }
    }
}