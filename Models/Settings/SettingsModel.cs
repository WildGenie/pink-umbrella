using System.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace PinkUmbrella.Models.Settings
{
    public class SettingsModel
    {
        [DisplayName("Host Display Name"), Description("The host's name to the rest of the world")]
        public string HostDisplayName { get; set; }

        [DisplayName("Accent Color"), Description("The primary color used to highlight parts of the site")]
        public string AccentColor { get; set; }






        [DisplayName("Peer Max Remember Count"), Description("The maximum amount of peers this site can remember")]
        public long PeerMaxRememberCount { get; set; } = 1000;

        [DisplayName("Peer Max Connect Count"), Description("The maximum amount of peers this site can connect to at once")]
        public long PeerMaxConnectCount { get; set; } = 50;
    }
}