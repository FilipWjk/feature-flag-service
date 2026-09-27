namespace FeatureFlags.Core;

public sealed class FeatureFlag
{
    public FeatureFlag(string name, bool enabled, int rolloutPercentage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Feature flag name must not be empty.", nameof(name));
        }

        if (rolloutPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rolloutPercentage),
                rolloutPercentage,
                $"Feature flag '{name}': RolloutPercentage must be between 0 and 100.");
        }

        Name = name;
        Enabled = enabled;
        RolloutPercentage = rolloutPercentage;
    }

    public string Name { get; }

    public bool Enabled { get; }

    public int RolloutPercentage { get; }
}