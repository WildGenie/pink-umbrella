using System;
using Poncho.Util;

namespace PinkUmbrella.Models.AhPushIt
{
    [IsDocumented]
    public class NotificationRecipient
    {
        public int Id { get; set; }

        public int ToUserId { get; set; }

        public int NotificationId { get; set; }

        public NotificationMethod Method { get; set; }

        public DateTime? WhenDelivered { get; set; }

        public DateTime? WhenViewed { get; set; }

        public DateTime? WhenDismissed { get; set; }
    }
}