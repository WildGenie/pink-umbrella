using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Developer
{
    public class UsersViewModel : BaseViewModel
    {
        public PaginatedModel<UserProfileModel> MostRecentlyCreatedUsers { get; set; }
    }
}