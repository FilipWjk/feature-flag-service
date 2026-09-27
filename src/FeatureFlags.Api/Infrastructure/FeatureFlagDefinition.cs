using FeatureFlags.Core;

namespace FeatureFlags.Api.Infrastructure;

public sealed class FeatureFlagDefinition
{
    public const string SectionName = "FeatureFlags";

    public string? Name { get; set; }

    public bool Enabled { get; set; }

    public int? RolloutPercentage { get; set; }

    public FeatureFlag ToFeatureFlag()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Feature flag name is required.");
        }

        if (RolloutPercentage is null)
        {
            throw new InvalidOperationException($"Feature flag '{Name}': RolloutPercentage is required.");
        }

        return new FeatureFlag(Name, Enabled, RolloutPercentage.Value);
    }
}