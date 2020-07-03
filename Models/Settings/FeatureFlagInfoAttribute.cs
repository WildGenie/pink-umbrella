using System;

namespace PinkUmbrella.Models.Settings
{
    public class FeatureFlagInfoAttribute: Attribute
    {
        public FeatureFlagInfoAttribute(string name, string description, FeatureFlagType featureFlagType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            FeatureFlagType = featureFlagType;
        }

        public string Name { get; }

        public string Description { get; }

        public FeatureFlagType FeatureFlagType { get; }
    }
}