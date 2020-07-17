using System.ComponentModel;
using PinkUmbrella.Util;
using Poncho.Util;

namespace PinkUmbrella.Models
{
    public enum InvitationType
    {
        [Name("Group Invite"), Description("Invite a user to a group")]
        AddMeToGroup,

        [Name("Register Invite"), Description("Invite a user to register on this site")]
        Register,

        [Name("Follow Invite"), Description("Invite a user to follow you")]
        FollowMe,
    }
}