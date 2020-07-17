using PinkUmbrella.Models;
using Poncho.Models;

namespace PinkUmbrella.ViewModels.Archive
{
    public class IndexViewModel : BaseViewModel
    {
        public PaginatedModel<ArchivedMediaModel> Items { get; set; }
    }
}