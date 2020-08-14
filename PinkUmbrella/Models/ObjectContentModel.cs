using System;
using Tides.Models;

namespace PinkUmbrella.Models
{
    public class ObjectContentModel
    {
        public int Id { get; set; }
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
    }
}