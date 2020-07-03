using System.Collections.Generic;
using PinkUmbrella.Models.Settings;

namespace PinkUmbrella.ViewModels.Admin
{
    public class FeatureFlagsViewModel: BaseViewModel
    {
        public List<FeatureFlagModel> FeatureFlags { get; set; } = new List<FeatureFlagModel>();
    }
}