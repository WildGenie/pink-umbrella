using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FeatureManagement;
using PinkUmbrella.Models.Settings;

namespace PinkUmbrella.Services
{
    public interface ISettingsService
    {
        IFeatureManager FeatureManager { get; }

        SettingsModel Site { get; }
        
        Task<List<FeatureFlagModel>> GetFeatureFlags();
        
        Task Update(string key, string value);
        
        Task UpdateToggle(string key, string value);
    }
}