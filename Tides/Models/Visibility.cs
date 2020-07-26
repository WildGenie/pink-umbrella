using System.ComponentModel;
using Tides.Util;

namespace Tides.Models
{
    public enum Visibility
    {
        [Name("Shadow Banned"), Description("Hidden to everybody, even the publisher, minus admins")]
        SHADOW_BANNED = -1,

        [Name("Hidden"), Description("Hidden to everybody")]
        HIDDEN = 0,

        [Name("Visible to world"), Description("Visible to the entire internet")]
        VISIBLE_TO_WORLD,

        [Name("Visible to registered"), Description("Visible to registered users")]
        VISIBLE_TO_REGISTERED,

        [Name("Visible to followers"), Description("Visible to your followers")]
        VISIBLE_TO_FOLLOWERS,
    }
}