using PinkUmbrella.Models.Peer;
using Poncho.Models.Peer;

namespace PinkUmbrella.ViewModels.Admin
{
    public class PeerViewModel
    {
        public PeerModel Peer { get; set; }

        public PeerStatsModel Stats { get; set; }
    }
}