using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Newtonsoft.Json.Linq;
using PinkUmbrella.Models.Settings;
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Services.Sql
{
    public class SettingsService : ISettingsService
    {
        public IFeatureManager FeatureManager { get; }
        public SettingsModel Site { get; } = new SettingsModel();
        private readonly StringRepository _strings;
        private readonly ILogger<SettingsService> _logger;
        private readonly IConfigurationSection _config;

        public SettingsService(ILogger<SettingsService> logger, IFeatureManager featureManager, StringRepository strings, IConfiguration configuration)
        {
            _logger = logger;
            FeatureManager = featureManager;
            _strings = strings;
            this._config = configuration.GetSection("SiteSettings");
            ConfigurationBinder.Bind(_config, this.Site);
            _config.GetReloadToken().RegisterChangeCallback(x => this.Reload(), null);
        }

        private void Reload()
        {
            _logger.LogInformation("Reloading settings since they changed");
            ConfigurationBinder.Bind(_config, this.Site);
            _config.GetReloadToken().RegisterChangeCallback(x => this.Reload(), null);
        }

        public async Task<List<FeatureFlagModel>> GetFeatureFlags()
        {
            var featureFlags = new List<FeatureFlagModel>();
            foreach (var feature in Enum.GetValues(typeof(FeatureFlags)).Cast<FeatureFlags>())
            {
                var key = feature.ToString();
                var ff = feature.GetType().GetMember(key).FirstOrDefault()?.GetCustomAttribute<FeatureFlagInfoAttribute>();
                featureFlags.Add(new FeatureFlagModel()
                {
                    Value = feature,
                    Enabled = await FeatureManager.IsEnabledAsync(key),
                    Title = ff?.Name ?? key,
                    Description = ff?.Description,
                    FeatureFlagType = ff?.FeatureFlagType ?? default,
                });
            }
            return featureFlags;
        }

        public async Task Update(string key, string value)
        {
            await UpdateSection("SiteSettings", key, value, null);
        }

        public async Task UpdateToggle(string key, string value)
        {
            if (Enum.TryParse(typeof(FeatureFlags), key, out var _))
            {
                switch (value?.ToLower())
                {
                    case "t":
                    case "true":
                    case "on":
                    case "yes":
                        await UpdateSection("FeatureManagement", key, true.ToString(), true);
                        break;
                    default:
                        await UpdateSection("FeatureManagement", key, false.ToString(), false);
                        break;
                }
            }
        }

        private async Task UpdateSection(string section, string key, string value, object defaultValue)
        {
            var oldConfig = $"appsettings.json";
            var newConfig = $"appsettings.json.new";
            var settings = JObject.Parse(await System.IO.File.ReadAllTextAsync(oldConfig));

            var sectionSettings = settings[section].Value<JObject>();
            if (sectionSettings.ContainsKey(key))
            {
                if (sectionSettings[key].Type == JTokenType.Boolean)
                {
                    sectionSettings[key] = _strings.IsPositive(value?.ToLower() ?? "off");
                }
                else if (sectionSettings[key].Type == JTokenType.Integer)
                {
                    sectionSettings[key] = long.Parse(value);
                }
                else
                {
                    sectionSettings[key] = new JValue(value);
                }
            }
            else if (defaultValue != null)
            {
                sectionSettings[key] = new JValue(defaultValue);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Invalid {section} key {key}");
            }

            await System.IO.File.WriteAllTextAsync(newConfig, settings.ToString());
            System.IO.File.Copy(oldConfig, $"{oldConfig}.{DateTime.Now.Ticks}");
            System.IO.File.Copy(newConfig, oldConfig, true);
        }
    }
}