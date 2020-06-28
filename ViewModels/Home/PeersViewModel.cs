using System.Collections.Generic;
using PinkUmbrella.Models.Peer;

namespace PinkUmbrella.ViewModels.Home
{
    public class PeersViewModel : BaseViewModel
    {
        public List<PeerModel> Peers { get; set; }
    }
}