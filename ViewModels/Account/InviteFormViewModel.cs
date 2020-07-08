using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Account
{
    public class InviteFormViewModel
    {
        [Description("Who the invite is for, if applicable. (leave blank for anyone)")]
        public int UserIdTo { get; set; }

        public InvitationType Type { get; set; }

        [Description("Add a note to the recipient of the invite"), Required, StringLength(1000, MinimumLength = 3)]
        public string Message { get; set; }

        [DisplayName("Days Valid For"), Description("How many days the invitation is valid for")]
        public int DaysValidFor { get; set; }
    }
}