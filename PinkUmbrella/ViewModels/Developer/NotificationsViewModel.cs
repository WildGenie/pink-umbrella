using PinkUmbrella.Models;
using PinkUmbrella.Models.AhPushIt;
using Poncho.Models;
using Poncho.Models.Public;

namespace PinkUmbrella.ViewModels.Developer
{
    public class NotificationsViewModel : BaseViewModel
    {
        public PaginatedModel<UserNotification> Items { get; set; }
        public int? SinceId { get; set; }
        public bool IncludeViewed { get; set; }
        public bool IncludeDismissed { get; set; }
    }
}