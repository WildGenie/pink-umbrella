using System.Collections.Generic;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.ViewModels.Account.SetupLoginMethod
{
    public class SetupRecoveryViewModel
    {
        public List<RecoveryKeyModel> RecoveryKeys { get; set; }
    }
}