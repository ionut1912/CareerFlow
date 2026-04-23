namespace CareerFlow.Core.Domain.Constants;

public static class CacheKeyConstants
{
    public static string CacheKeyDocAnalyze(string fileName) => $"course:analyze:{fileName}";

    public static string CacheKeyChapter(string fileName) => $"course:chapters:{fileName}";

    public static string CacheKeySkeleton(string topic) => $"course:skeleton:{topic}";

    public static string CacheKeyExpand(string topic) => $"course:expand:{topic}";

    public static string CacheKeyState(string state) => $"oauth_state:{state}";

    public static string CacheKeyLegal(string type) => $"legal:{type.ToLowerInvariant()}";
}
