using PinkUmbrella.Models;
using Poncho.Models;

namespace PinkUmbrella.ViewModels.Admin
{
    public class UsersViewModel : BaseViewModel
    {
        public PaginatedModel<UserProfileModel> MostRecentlyCreatedUsers { get; set; }
    }
}