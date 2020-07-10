using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkUmbrella.Models.Auth
{
    public class RecoveryKeyModel
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Label { get; set; }

        public DateTime WhenCreated { get; set; }

        [DefaultValue(null)]
        public DateTime? WhenShown { get; set; }

        [DefaultValue(null)]
        public DateTime? WhenUsed { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string Code { get; set; }


        [NotMapped]
        public bool ShowCode => WhenShown.HasValue && (DateTime.UtcNow.Subtract(WhenShown.Value)).TotalMinutes < 1;
    }
}