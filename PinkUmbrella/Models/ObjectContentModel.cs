using System;
using Tides.Models;

namespace PinkUmbrella.Models
{
    public class ObjectContentModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Handle { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string MediaType { get; set; }
        public Visibility Visibility { get; set; }
        public DateTime Published { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? Deleted { get; set; }
        public bool IsMature { get; set; }



        public string AttachmentCSV { get; set; }
        public string AttributedToCSV { get; set; }
        public string AudienceCSV { get; set; }
        public string ContextCSV { get; set; }
        public string IconCSV { get; set; }
        public string ImageCSV { get; set; }
        public string InReplyToCSV { get; set; }
        public string LocationCSV { get; set; }
        public string RepliesCSV { get; set; }
        public string TagCSV { get; set; }



        public string FromCSV { get; set; }
        public string ToCSV { get; set; }
        public string BtoCSV { get; set; }
        public string CcCSV { get; set; }
        public string BccCSV { get; set; }
    }
}