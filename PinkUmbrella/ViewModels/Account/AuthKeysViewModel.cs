using System.Collections.Generic;
using PinkUmbrella.Models.Auth;
using Poncho.Models.Auth;

namespace PinkUmbrella.ViewModels.Account
{
    public class AuthKeysViewModel: BaseViewModel
    {
        public List<PublicKey> Keys { get; set; } = new List<PublicKey>();

        public AddKeyViewModel NewKey { get; set; } = new AddKeyViewModel();

        public GenKeyViewModel GenNewKey { get; set; } = new GenKeyViewModel();
    }
}