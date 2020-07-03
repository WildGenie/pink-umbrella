namespace PinkUmbrella.Models.Settings
{
    public class FeatureFlagModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public FeatureFlags Value { get; set; }
        public FeatureFlagType FeatureFlagType { get; set; }
    }
}