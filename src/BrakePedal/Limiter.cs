using System;

namespace BrakePedal
{
    public class Limiter
    {
        public long Count { get; set; }
        public TimeSpan Period { get; set; }
        public TimeSpan? LockDuration { get; set; }

        public Limiter Limit(long count)
        {
            Count = count;
            return this;
        }

        public Limiter Over(long seconds)
        {
            return Over(TimeSpan.FromSeconds(seconds));
        }

        public Limiter Over(TimeSpan span)
        {
            Period = span;
            return this;
        }

        public Limiter PerSecond(long count)
        {
            return Limit(count).Over(1);
        }

        public Limiter PerMinute(long count)
        {
            return Limit(count).Over(60);
        }

        public Limiter PerHour(long count)
        {
            return Limit(count).Over(TimeSpan.FromHours(1));
        }

        public Limiter PerDay(long count)
        {
            return Limit(count).Over(TimeSpan.FromDays(1));
        }

        public Limiter LockFor(long seconds)
        {
            return LockFor(TimeSpan.FromSeconds(seconds));
        }

        public Limiter LockFor(TimeSpan span)
        {
            LockDuration = span;
            return this;
        }
    }
}