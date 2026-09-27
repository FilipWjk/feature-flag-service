using System.Collections.Frozen;
using FeatureFlags.Core;

namespace FeatureFlags.Api.Infrastructure;

public sealed class InMemoryFeatureFlagRepository : IFeatureFlagRepository
{
    // * Flags are loaded once at startup; FrozenDictionary makes concurrent lookups immutable and optimized for reads.
    private readonly FrozenDictionary<string, FeatureFlag> _flags;

    public InMemoryFeatureFlagRepository(IEnumerable<FeatureFlag> flags)
    {
        var flagsByName = new Dictionary<string, FeatureFlag>(StringComparer.OrdinalIgnoreCase);
        foreach (var flag in flags)
        {
            if (!flagsByName.TryAdd(flag.Name, flag))
            {
                throw new ArgumentException(
                    $"Duplicate feature flag name '{flag.Name}' (names are case-insensitive).",
                    nameof(flags));
            }
        }

        _flags = flagsByName.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public FeatureFlag? Find(string name) => _flags.GetValueOrDefault(name);
}