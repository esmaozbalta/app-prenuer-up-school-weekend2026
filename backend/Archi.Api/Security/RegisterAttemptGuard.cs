using System.Collections.Concurrent;

namespace Archi.Api.Security;

public static class RegisterAttemptGuard
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(10);
    private static readonly ConcurrentDictionary<string, AttemptInfo> Attempts = new();

    public static bool IsLocked(string key)
    {
        if (!Attempts.TryGetValue(key, out var info))
        {
            return false;
        }

        if (info.LockedUntilUtc is null)
        {
            return false;
        }

        if (info.LockedUntilUtc <= DateTime.UtcNow)
        {
            Attempts.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    public static void RegisterFailure(string key)
    {
        Attempts.AddOrUpdate(
            key,
            _ => AttemptInfo.FailedOnce(DateTime.UtcNow),
            (_, existing) => existing.Increment(DateTime.UtcNow, MaxFailedAttempts, LockDuration));
    }

    public static void RegisterSuccess(string key)
    {
        Attempts.TryRemove(key, out _);
    }

    private sealed record AttemptInfo(int FailedCount, DateTime? LockedUntilUtc)
    {
        public static AttemptInfo FailedOnce(DateTime nowUtc) => new(1, null);

        public AttemptInfo Increment(
            DateTime nowUtc,
            int maxFailedAttempts,
            TimeSpan lockDuration)
        {
            var nextCount = FailedCount + 1;
            DateTime? lockedUntil = nextCount >= maxFailedAttempts
                ? nowUtc.Add(lockDuration)
                : null;
            return new AttemptInfo(nextCount, lockedUntil);
        }
    }
}
