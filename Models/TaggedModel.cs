using System;
using System.ComponentModel.DataAnnotations.Schema;
using PinkUmbrella.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class TaggedModel
    {
        public int Id { get; set; }
        public DateTime WhenCreated { get; set; }
        public int UserId { get; set; }
        public int ToId { get; set; }
        public int TagId { get; set; }

        [NotMapped]
        public TagModel Tag { get; set; }
    }
}