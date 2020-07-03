using System;

namespace PinkUmbrella.Models.Auth
{
    public class IPBlockModel
    {
        public int Id { get; set; }

        public DateTime WhenBlocked { get; set; }
        
        public int ByUserId { get; set; }
        
        public long IPId { get; set; }

        public string Reason { get; set; }
    }
}