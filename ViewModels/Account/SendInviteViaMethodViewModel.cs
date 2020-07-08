using System.Collections.Generic;

namespace PinkUmbrella.ViewModels.Account
{
    public class SendInviteViaMethodViewModel : BaseViewModel
    {
        public string QRCodeImageAsBase64 { get; set; }
        
        public string RawString { get; set; }
        
        public string Link { get; internal set; }
    }
}