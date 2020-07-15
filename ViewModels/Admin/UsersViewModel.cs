using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Admin
{
    public class UsersViewModel : BaseViewModel
    {
        public PaginatedModel<UserProfileModel> MostRecentlyCreatedUsers { get; set; }
    }
}