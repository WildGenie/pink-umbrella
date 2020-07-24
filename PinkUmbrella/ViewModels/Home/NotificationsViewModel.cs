using PinkUmbrella.Models;
using PinkUmbrella.Models.AhPushIt;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.ViewModels.Home
{
    public class NotificationsViewModel : BaseViewModel
    {
        public PaginatedModel<UserNotification> Items { get; set; }
        public int? SinceId { get; set; }
        public bool IncludeViewed { get; set; }
        public bool IncludeDismissed { get; set; }
    }
}