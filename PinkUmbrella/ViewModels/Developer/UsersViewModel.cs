using PinkUmbrella.Models;
using Tides.Models;

namespace PinkUmbrella.ViewModels.Developer
{
    public class UsersViewModel : BaseViewModel
    {
        public PaginatedModel<UserProfileModel> MostRecentlyCreatedUsers { get; set; }
    }
}