using FeatureFlags.Core;
using FeatureFlags.Core.Services;

namespace FeatureFlags.Core.Tests;

public class FeatureFlagServiceTests
{
    [Fact]
    public void UnknownFeature_ReturnsNotFound()
    {
        var evaluation = CreateEvaluator().Evaluate("Unknown", "123");

        Assert.False(evaluation.FeatureFound);
    }

    [Fact]
    public void DisabledFeature_IsDisabledForEveryUser()
    {
        var evaluator = CreateEvaluator(new FeatureFlag("Legacy", enabled: false, rolloutPercentage: 100));

        Assert.False(evaluator.Evaluate("Legacy", "first").IsEnabled);
        Assert.False(evaluator.Evaluate("Legacy", "second").IsEnabled);
    }

    [Fact]
    public void FullRollout_IsEnabledForEveryUser()
    {
        var evaluator = CreateEvaluator(new FeatureFlag("DarkMode", enabled: true, rolloutPercentage: 100));

        Assert.True(evaluator.Evaluate("DarkMode", "first").IsEnabled);
        Assert.True(evaluator.Evaluate("DarkMode", "second").IsEnabled);
    }

    [Fact]
    public void PartialRollout_IsDeterministicAndCaseInsensitive()
    {
        var evaluator = CreateEvaluator(new FeatureFlag("NewDashboard", enabled: true, rolloutPercentage: 20));

        var original = evaluator.Evaluate("NewDashboard", "user-42");
        var repeated = evaluator.Evaluate("NewDashboard", "user-42");
        var differentCase = evaluator.Evaluate("newdashboard", "user-42");

        Assert.Equal(original, repeated);
        Assert.Equal(original, differentCase);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void InvalidRollout_IsRejected(int rolloutPercentage)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new FeatureFlag("Invalid", enabled: true, rolloutPercentage));
    }

    private static FeatureFlagService CreateEvaluator(params FeatureFlag[] flags) =>
        new(new Repository(flags));

    private sealed class Repository(IEnumerable<FeatureFlag> flags) : IFeatureFlagRepository
    {
        private readonly Dictionary<string, FeatureFlag> _flags =
            flags.ToDictionary(flag => flag.Name, StringComparer.OrdinalIgnoreCase);

        public FeatureFlag? Find(string name) => _flags.GetValueOrDefault(name);
    }
}