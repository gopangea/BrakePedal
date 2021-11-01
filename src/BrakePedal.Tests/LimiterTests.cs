using System;
using Xunit;

namespace BrakePedal.Tests
{
    public class LimiterTests
    {
        public class LimitMethod
        {
            [Fact]
            public void CallWithNumber_SetCountReturnObject()
            {
                var limiter = new Limiter();
                Limiter returned = limiter.Limit(10);

                Assert.Equal(10, limiter.Count);
                Assert.Equal(returned, limiter);
            }
        }

        public class LockForMethod
        {
            [Fact]
            public void CallWithSeconds_SetLockDuration()
            {
                Limiter limiter = new Limiter()
                    .LockFor(1);

                Assert.Equal(TimeSpan.FromSeconds(1), limiter.LockDuration);
            }

            [Fact]
            public void CallWithTimeSpan_SetLockDuration()
            {
                Limiter limiter = new Limiter()
                    .LockFor(TimeSpan.FromSeconds(1));

                Assert.Equal(TimeSpan.FromSeconds(1), limiter.LockDuration);
            }
        }

        public class OverMethod
        {
            [Fact]
            public void CallWithSeconds_SetPeriod()
            {
                Limiter limiter = new Limiter()
                    .Over(10);

                Assert.Equal(TimeSpan.FromSeconds(10), limiter.Period);
            }

            [Fact]
            public void CallWithTimeSpan_SetPeriod()
            {
                Limiter limiter = new Limiter()
                    .Over(TimeSpan.FromSeconds(10));

                Assert.Equal(TimeSpan.FromSeconds(10), limiter.Period);
            }
        }

        public class PerMethods
        {
            [Fact]
            public void PerSecond_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerSecond(10);

                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromSeconds(1), limiter.Period);
            }

            [Fact]
            public void PerMinute_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerMinute(10);

                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromMinutes(1), limiter.Period);
            }

            [Fact]
            public void PerHour_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerHour(10);

                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromHours(1), limiter.Period);
            }

            [Fact]
            public void PerDay_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerDay(10);

                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromDays(1), limiter.Period);
            }
        }
    }
}