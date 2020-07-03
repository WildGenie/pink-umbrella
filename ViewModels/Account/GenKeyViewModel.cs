using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.ViewModels.Account
{
    public class GenKeyViewModel
    {
        public PrivateKey AuthKey { get; set; } = new PrivateKey();
    }
}