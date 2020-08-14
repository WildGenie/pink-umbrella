using Estuary.Core;

namespace PinkUmbrella.Models.AhPushIt
{
    public class UserNotification
    {
        public UserNotification(NotificationRecipient r, Notification n)
        {
            User = r;
            Notif = n;
        }

        public NotificationRecipient User { get; set; }

        public Notification Notif { get; set; }
        
        public BaseObject FromUser { get; set; }
    }
}