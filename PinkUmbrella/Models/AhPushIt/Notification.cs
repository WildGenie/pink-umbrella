using System;
using PinkUmbrella.Util;
using Poncho.Models;
using Poncho.Util;

namespace PinkUmbrella.Models.AhPushIt
{
    [IsDocumented]
    public class Notification
    {
        public int Id { get; set; }

        public NotificationType Type { get; set; }

        public NotificationPriority Priority { get; set; }

        public int FromUserId { get; set; }

        public long FromPeerId { get; set; }
        
        public ReactionSubject Subject { get; set; }

        public int SubjectId { get; set; }

        public DateTime WhenCreated { get; set; }

        public string DataJson { get; set; }

        public int RecipientCount { get; set; }

        public int DeliveryCount { get; set; }

        public int ViewCount { get; set; }

        public int DismissCount { get; set; }
    }
}