using Tides.Models.Peer;

namespace PinkUmbrella.ViewModels.Admin
{
    public class PeerViewModel
    {
        public Estuary.Actors.Peer Peer { get; set; }

        public PeerStatsModel Stats { get; set; }
    }
}