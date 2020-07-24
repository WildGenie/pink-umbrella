using PinkUmbrella.Models.Auth;
using Tides.Models.Auth;

namespace PinkUmbrella.ViewModels.Account
{
    public class GenKeyViewModel
    {
        public PrivateKey AuthKey { get; set; } = new PrivateKey();
    }
}