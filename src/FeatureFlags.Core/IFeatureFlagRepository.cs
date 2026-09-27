namespace FeatureFlags.Core;

public interface IFeatureFlagRepository
{
    FeatureFlag? Find(string name);
}