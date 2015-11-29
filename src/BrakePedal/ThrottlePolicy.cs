using System;
using System.Collections.Generic;
using System.Linq;

namespace BrakePedal
{
    public class ThrottlePolicy : IThrottlePolicy
    {
        private readonly IThrottleRepository _repository;

        private List<Limiter> _limits;

        private string[] _prefixes;

        public ThrottlePolicy()
            : this(new MemoryThrottleRepository())
        {
        }

        public ThrottlePolicy(IThrottleRepository repository)
        {
            Limiters = new List<Limiter>();
            _repository = repository;
        }

        public long? PerSecond
        {
            get { return GetLimiterCount(TimeSpan.FromSeconds(1)); }
            set { SetLimiter(TimeSpan.FromSeconds(1), value); }
        }

        public long? PerMinute
        {
            get { return GetLimiterCount(TimeSpan.FromMinutes(1)); }
            set { SetLimiter(TimeSpan.FromMinutes(1), value); }
        }

        public long? PerHour
        {
            get { return GetLimiterCount(TimeSpan.FromHours(1)); }
            set { SetLimiter(TimeSpan.FromHours(1), value); }
        }

        public long? PerDay
        {
            get { return GetLimiterCount(TimeSpan.FromDays(1)); }
            set { SetLimiter(TimeSpan.FromDays(1), value); }
        }

        public ICollection<Limiter> Limiters
        {
            get { return _limits; }
            set { _limits = new List<Limiter>(value); }
        }

        public string Name { get; set; }

        public string[] Prefixes
        {
            get { return _prefixes; }
            set
            {
                _prefixes = value;
                _repository.PolicyIdentityValues = _prefixes;
            }
        }

        public bool IsThrottled(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsThrottled;
        }

        public bool IsLocked(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsLocked;
        }

        public CheckResult Check(IThrottleKey key, bool increment = true)
        {
            foreach (Limiter limiter in Limiters)
            {
                var result = new CheckResult
                {
                    IsThrottled = false,
                    IsLocked = false,
                    ThrottleKey = _repository.CreateThrottleKey(key, limiter),
                    Limiter = limiter
                };

                if (limiter.LockDuration.HasValue)
                {
                    result.LockKey = _repository.CreateLockKey(key, limiter);
                    if (_repository.LockExists(key, limiter))
                    {
                        result.IsLocked = true;
                        return result;
                    }
                }

                // Short-circuit this loop if the
                // limit value isn't valid
                if (limiter.Count <= 0)
                    continue;

                long? counter = _repository.GetThrottleCount(key, limiter);

                if (counter.HasValue
                    && counter.Value >= limiter.Count)
                {
                    if (limiter.LockDuration.HasValue)
                    {
                        _repository.SetLock(key, limiter);
                        _repository.RemoveThrottle(key, limiter);
                    }

                    result.IsThrottled = true;
                    return result;
                }

                if (increment)
                    _repository.AddOrIncrementWithExpiration(key, limiter);
            }

            return CheckResult.NotThrottled;
        }

        private void SetLimiter(TimeSpan span, long? count)
        {
            Limiter item = Limiters.FirstOrDefault(l => l.Period == span);
            if (item != null)
                _limits.Remove(item);

            if (!count.HasValue)
                return;

            item = new Limiter
            {
                Count = count.Value,
                Period = span
            };

            _limits.Add(item);
        }

        private long? GetLimiterCount(TimeSpan span)
        {
            Limiter item = Limiters.FirstOrDefault(l => l.Period == span);
            long? result = null;

            if (item != null)
                result = item.Count;

            return result;
        }

        public bool Check(IThrottleKey key, out CheckResult result, bool increment = true)
        {
            result = Check(key, increment);
            return result.IsThrottled;
        }
    }
}