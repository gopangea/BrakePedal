using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace BrakePedal
{
    public class MemoryThrottleRepository : IThrottleRepository
    {
        private readonly ObjectCache _store;

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
            var item = _store.Get(id) as long?;
            if (item.HasValue)
                item = item++;
            else
                item = 1;

            DateTime expiration = DateTime.UtcNow.Add(limiter.Period);
            _store.Set(id, item, expiration);
        }

        public void SetLock(IThrottleKey key, Limiter limiter)
        {
            string throttleId = CreateThrottleKey(key, limiter);
            _store.Remove(throttleId);

            string lockId = CreateLockKey(key, limiter);
            DateTime expiration = DateTime.UtcNow.Add(limiter.LockDuration.Value);
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

            string countKey = TimeSpanToFriendlyString(limiter.Period);
            values.Add(countKey);

            string id = string.Join(":", values);
            return id;
        }

        public string CreateThrottleKey(IThrottleKey key, Limiter limiter)
        {
            List<object> values = CreateBaseKeyValues(key, limiter);

            string lockKeySuffix = TimeSpanToFriendlyString(limiter.LockDuration.Value);
            values.Add("lock");
            values.Add(lockKeySuffix);

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
    }
}