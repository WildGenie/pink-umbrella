using Tides.Util;

namespace PinkUmbrella.Models.AhPushIt
{
    [IsDocumented]
    public class NotificationMethodSetting
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public NotificationType Type { get; set; }
        
        public NotificationMethod Method { get; set; }
        
        public bool Enabled { get; set; }
    }
}