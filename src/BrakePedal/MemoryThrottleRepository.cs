using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace BrakePedal
{
    public class MemoryThrottleRepository : IThrottleRepository
    {
        private readonly ObjectCache _store;

        // Setup as a function to allow for unit testing
        public Func<DateTime> CurrentDate = () => DateTime.UtcNow;

        public MemoryThrottleRepository(ObjectCache cache)
        {
            _store = cache;
        }

        public MemoryThrottleRepository()
        {
            _store = new MemoryCache("throttleRepository");
        }

        public object[] PolicyIdentityValues { get; set; }

        public long? GetThrottleCount(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);
            return _store.Get(id) as long?;
        }

        public void AddOrIncrementWithExpiration(IThrottleKey key, Limiter limiter)
        {
            string id = CreateThrottleKey(key, limiter);
            var cacheItem = _store.Get(id) as ThrottleCacheItem;

            if (cacheItem != null)
            {
                cacheItem.Count = cacheItem.Count + 1;
            }
            else
            {
                cacheItem = new ThrottleCacheItem()
                {
                    Count = 1,
                    Expiration = CurrentDate().Add(limiter.Period)
                };
            }

            _store.Set(id, cacheItem, cacheItem.Expiration);
        }

        public void SetLock(IThrottleKey key, Limiter limiter)
        {
            string throttleId = CreateThrottleKey(key, limiter);
            _store.Remove(throttleId);

            string lockId = CreateLockKey(key, limiter);
            DateTime expiration = CurrentDate().Add(limiter.LockDuration.Value);
            _store.Set(lockId, true, expiration);
        }

        public bool LockExists(IThrottleKey key, Limiter limiter)
        {
            string lockId = CreateLockKey(key, limiter);
            return _store.Contains(lockId);
        }

        public void RemoveThrottle(IThrottleKey key, Limiter limiter)
        {
            string lockId = CreateThrottleKey(key, limiter);
            _store.Remove(lockId);
        }

        public string CreateLockKey(IThrottleKey key, Limiter limiter)
        {
            List<object> values = CreateBaseKeyValues(key, limiter);

            string lockKeySuffix = TimeSpanToFriendlyString(limiter.LockDuration.Value);
            values.Add("lock");
            values.Add(lockKeySuffix);

            string id = string.Join(":", values);
            return id;
        }

        public string CreateThrottleKey(IThrottleKey key, Limiter limiter)
        {
            List<object> values = CreateBaseKeyValues(key, limiter);

            string countKey = TimeSpanToFriendlyString(limiter.Period);
            values.Add(countKey);

            // Using the Unix timestamp to the key allows for better
            // precision when querying a key from Redis
            if (limiter.Period.TotalSeconds == 1)
                values.Add(GetUnixTimestamp());

            string id = string.Join(":", values);
            return id;
        }

        private List<object> CreateBaseKeyValues(IThrottleKey key, Limiter limiter)
        {
            List<object> values = key.Values.ToList();
            if (PolicyIdentityValues != null && PolicyIdentityValues.Length > 0)
                values.InsertRange(0, PolicyIdentityValues);

            return values;
        }

        private string TimeSpanToFriendlyString(TimeSpan span)
        {
            var items = new List<string>();
            Action<double, string> ifNotZeroAppend = (value, key) =>
            {
                if (value != 0)
                    items.Add(string.Concat(value, key));
            };

            ifNotZeroAppend(span.Days, "d");
            ifNotZeroAppend(span.Hours, "h");
            ifNotZeroAppend(span.Minutes, "m");
            ifNotZeroAppend(span.Seconds, "s");

            return string.Join("", items);
        }

        private long GetUnixTimestamp()
        {
            TimeSpan timeSpan = (CurrentDate() - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        [Serializable]
        public class ThrottleCacheItem
        {
            public long Count { get; set; }

            public DateTime Expiration { get; set; }
        }
    }
}