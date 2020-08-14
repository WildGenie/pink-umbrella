using System;
using Tides.Util;

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
        
        public string SubjectType { get; set; }

        public int SubjectId { get; set; }

        public DateTime WhenCreated { get; set; }

        public string DataJson { get; set; }
    }
}