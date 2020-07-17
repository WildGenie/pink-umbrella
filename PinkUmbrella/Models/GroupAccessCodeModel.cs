using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkUmbrella.Models
{
    public class GroupAccessCodeModel
    {
        public int Id { get; set; }

        [DisplayName("When Created"), Description("When the access code was generated")]
        public DateTime WhenCreated { get; set; }

        [DisplayName("When Expires"), Description("When the access code expires")]
        public DateTime WhenExpires { get; set; }

        [DefaultValue(null), DisplayName("When Consumed"), Description("When the access code was claimed")]
        public DateTime? WhenConsumed { get; set; }

        [Description("The access code to grant access to the group")]
        public string Code { get; set; }

        [DisplayName("Created By User Id"), Description("Who generated the access code")]
        public int CreatedByUserId { get; set; }

        [DisplayName("For User Id"), Description("Who the generated the access code is for")]
        public int ForUserId { get; set; }

        [DisplayName("Group Name"), Description("Which group the access code is for")]
        public string GroupName { get; set; }

        [DisplayName("Invitation Type"), Description("What the access code is used for")]
        public InvitationType Type { get; set; }

        [NotMapped]
        public UserProfileModel ForUser { get; set; }


        public override string ToString()
        {
            return $"{CreatedByUserId} created \"{Code} for {ForUserId} to access {Type} {GroupName} at {WhenCreated}. Expires {WhenExpires}, consumed {(WhenConsumed?.ToString() ?? "never")}\"";
        }
    }
}