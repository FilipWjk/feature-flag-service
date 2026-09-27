using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace FeatureFlags.Core;

public static class RolloutBucketer
{
    public const int BucketCount = 100;

    public static int GetBucket(string featureName, string userId)
    {
        // For example, "NewDashboard" and user "102" become "newdashboard:102".
        // Normalization keeps case-insensitive feature lookups in the same rollout group.
        var input = Encoding.UTF8.GetBytes($"{featureName.ToLowerInvariant()}:{userId}");
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(input, hash);
        var bucket = (int)(BinaryPrimitives.ReadUInt32BigEndian(hash) % BucketCount);
        return bucket;
    }
}