using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Admin
{
    public class ExceptionsViewModel : BaseViewModel
    {
        public PaginatedModel<LoggedExceptionModel> Exceptions { get; set; }
    }
}