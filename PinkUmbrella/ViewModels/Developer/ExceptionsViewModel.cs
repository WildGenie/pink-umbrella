using System.Threading.Tasks;
using PinkUmbrella.Models;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.ViewModels.Developer
{
    public class ExceptionsViewModel : BaseViewModel
    {
        public PaginatedModel<LoggedExceptionModel> Exceptions { get; set; }
    }
}