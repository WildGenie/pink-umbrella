using System.Threading.Tasks;
using PinkUmbrella.Models;
using Poncho.Models;
using Poncho.Models.Public;

namespace PinkUmbrella.ViewModels.Developer
{
    public class ExceptionsViewModel : BaseViewModel
    {
        public PaginatedModel<LoggedExceptionModel> Exceptions { get; set; }
    }
}