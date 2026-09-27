namespace FeatureFlags.Core.Services;

public sealed class FeatureFlagService(IFeatureFlagRepository repository)
{
    public FeatureEvaluation Evaluate(string featureName, string userId)
    {
        var flag = repository.Find(featureName);
        if (flag is null)
        {
            return FeatureEvaluation.NotFound;
        }

        if (!flag.Enabled)
        {
            return FeatureEvaluation.Evaluated(false);
        }

        if (flag.RolloutPercentage == 100)
        {
            return FeatureEvaluation.Evaluated(true);
        }

        return FeatureEvaluation.Evaluated(
            RolloutBucketer.GetBucket(flag.Name, userId) < flag.RolloutPercentage);
    }
}