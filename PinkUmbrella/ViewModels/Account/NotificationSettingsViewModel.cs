using System.Collections.Generic;
using PinkUmbrella.Models.AhPushIt;

namespace PinkUmbrella.ViewModels.Account
{
    public class NotificationSettingsViewModel : BaseViewModel
    {
        public List<NotificationMethodSetting> Settings { get; set; }
    }
}