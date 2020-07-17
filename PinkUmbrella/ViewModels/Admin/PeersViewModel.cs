using System.Collections.Generic;
using PinkUmbrella.Models.Peer;

namespace PinkUmbrella.ViewModels.Admin
{
    public class PeersViewModel : BaseViewModel
    {
        public List<PeerViewModel> Peers { get; set; }
    }
}