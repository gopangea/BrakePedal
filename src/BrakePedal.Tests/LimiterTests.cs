using FluentAssertions;
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

                limiter.Count.ShouldBeEquivalentTo(10);
                returned.ShouldBeEquivalentTo(limiter);
            }
        }

        public class LockForMethod
        {
            [Fact]
            public void CallWithSeconds_SetLockDuration()
            {
                Limiter limiter = new Limiter()
                    .LockFor(1);

                limiter.LockDuration.ShouldBeEquivalentTo(TimeSpan.FromSeconds(1));
            }

            [Fact]
            public void CallWithTimeSpan_SetLockDuration()
            {
                Limiter limiter = new Limiter()
                    .LockFor(TimeSpan.FromSeconds(1));

                limiter.LockDuration.ShouldBeEquivalentTo(TimeSpan.FromSeconds(1));
            }
        }

        public class OverMethod
        {
            [Fact]
            public void CallWithSeconds_SetPeriod()
            {
                Limiter limiter = new Limiter()
                    .Over(10);

                limiter.Period.ShouldBeEquivalentTo(TimeSpan.FromSeconds(10));
            }

            [Fact]
            public void CallWithTimeSpan_SetPeriod()
            {
                Limiter limiter = new Limiter()
                    .Over(TimeSpan.FromSeconds(10));

                limiter.Period.ShouldBeEquivalentTo(TimeSpan.FromSeconds(10));
            }
        }

        public class PerMethods
        {
            [Fact]
            public void PerSecond_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerSecond(10);

                limiter.Count.ShouldBeEquivalentTo(10);
                limiter.Period.ShouldBeEquivalentTo(TimeSpan.FromSeconds(1));
            }

            [Fact]
            public void PerMinute_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerMinute(10);

                limiter.Count.ShouldBeEquivalentTo(10);
                limiter.Period.ShouldBeEquivalentTo(TimeSpan.FromMinutes(1));
            }

            [Fact]
            public void PerHour_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerHour(10);

                limiter.Count.ShouldBeEquivalentTo(10);
                limiter.Period.ShouldBeEquivalentTo(TimeSpan.FromHours(1));
            }

            [Fact]
            public void PerDay_SetCountAndPeriod()
            {
                Limiter limiter = new Limiter()
                    .PerDay(10);

                limiter.Count.ShouldBeEquivalentTo(10);
                limiter.Period.ShouldBeEquivalentTo(TimeSpan.FromDays(1));
            }
        }
    }
}