using PinkUmbrella.Models.Auth;
using Tides.Models.Auth;

namespace PinkUmbrella.ViewModels.Account
{
    public class LoginViaPublicKeyViewModel : BaseViewModel
    {
        public string Key { get; set; }
        public string Challenge { get; set; }
        public string Answer { get; set; }
        public AuthType Type { get; set; }
    }
}