namespace FeatureFlags.Core;

public readonly record struct FeatureEvaluation
{
    private FeatureEvaluation(bool featureFound, bool isEnabled)
    {
        FeatureFound = featureFound;
        IsEnabled = isEnabled;
    }

    public bool FeatureFound { get; }

    public bool IsEnabled { get; }

    public static FeatureEvaluation NotFound { get; } = new(featureFound: false, isEnabled: false);

    public static FeatureEvaluation Evaluated(bool isEnabled) => new(featureFound: true, isEnabled);
}