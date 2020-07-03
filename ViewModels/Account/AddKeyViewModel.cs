using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.ViewModels.Account
{
    public class AddKeyViewModel
    {
        public AuthKeyOptions AuthKey { get; set; } = new AuthKeyOptions();
        public PublicKey PublicKey { get; set; }
    }
}