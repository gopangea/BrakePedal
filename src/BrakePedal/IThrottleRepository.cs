namespace BrakePedal
{
    public interface IThrottleRepository
    {
        object[] PolicyIdentityValues { get; set; }

        long? GetThrottleCount(IThrottleKey key, Limiter limiter);

        void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter);

        void SetLock(IThrottleKey key, Limiter limiter);

        bool LockExists(IThrottleKey key, Limiter limiter);

        void RemoveThrottle(IThrottleKey key, Limiter limiter);

        string CreateThrottleKey(IThrottleKey key, Limiter limiter);

        string CreateLockKey(IThrottleKey key, Limiter limiter);
    }
}