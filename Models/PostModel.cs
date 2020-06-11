using System;

namespace seattle.Models
{
    public class PostModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Visibility Visibility { get; set; }
        public PostType PostType { get; set; }
        public bool IsReply { get; set; }
        public Visibility WhoCanReply { get; set; }
        public DateTime WhenCreated { get; set; }
        public DateTime? WhenDeleted { get; set; }
        public int DeletedByUserId { get; set; }
        public bool ContainsProfanity { get; set; }
        public bool ShadowBanned { get; set; }
        public string Content { get; set; }
    }
}