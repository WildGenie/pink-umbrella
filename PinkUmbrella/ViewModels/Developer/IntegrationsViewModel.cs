using System.Collections.Generic;
using PinkUmbrella.Models.Auth;
using Tides.Models.Auth;

namespace PinkUmbrella.ViewModels.Developer
{
    public class IntegrationsViewModel : BaseViewModel
    {
        public List<ApiAuthKeyModel> Items { get; set; }

        public AuthType authType { get; set; }

        public string clientPublicKey { get; set; }
    }
}