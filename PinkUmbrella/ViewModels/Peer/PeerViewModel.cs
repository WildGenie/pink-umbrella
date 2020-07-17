using PinkUmbrella.Models.Peer;
using Poncho.Models.Peer;

namespace PinkUmbrella.ViewModels.Peer
{
    public class PeerViewModel : BaseViewModel
    {
        public PeerModel Peer { get; set; }
        
        public object ProxiedViewModel { get; set; }

        public string ViewName { get; set; }
    }
}