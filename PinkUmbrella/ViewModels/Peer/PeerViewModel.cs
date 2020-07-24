using PinkUmbrella.Models.Peer;
using Tides.Models.Peer;

namespace PinkUmbrella.ViewModels.Peer
{
    public class PeerViewModel : BaseViewModel
    {
        public PeerModel Peer { get; set; }
        
        public object ProxiedViewModel { get; set; }

        public string ViewName { get; set; }
    }
}