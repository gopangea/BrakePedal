using NSubstitute;
using System;
using System.Linq;
using Xunit;

namespace BrakePedal.Tests
{
    public class ThrottlePolicyTests
    {
        public class CheckMethod
        {
            [Fact]
            public void NoLimits_ReturnsNotThrottled()
            {
                // Arrange
                var policy = new ThrottlePolicy();
                var key = Substitute.For<IThrottleKey>();

                // Act
                CheckResult result = policy.Check(key);

                // Assert
                Assert.Equal(CheckResult.NotThrottled.IsThrottled, result.IsThrottled);
                Assert.Equal(CheckResult.NotThrottled.IsLocked, result.IsLocked);
            }

            [Fact]
            public void Locked_ReturnsLocked()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    LockDuration = TimeSpan.FromSeconds(1)
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(true);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                CheckResult result = policy.Check(key);

                // Assert
                Assert.True(result.IsLocked);
                Assert.Equal(limit, result.Limiter);
            }

            [Fact]
            public void ZeroLimit_ReturnsNotThrottled()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    Count = 0
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(false);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                CheckResult result = policy.Check(key);

                // Assert
                Assert.Equal(CheckResult.NotThrottled.IsThrottled, result.IsThrottled);
                Assert.Equal(CheckResult.NotThrottled.IsLocked, result.IsLocked);
            }

            [Fact]
            public void LimitReached_ReturnsThrottled()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    Count = 1
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(false);

                repo.GetThrottleCount(key, limit)
                    .Returns(1);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                CheckResult result = policy.Check(key);

                // Assert
                Assert.True(result.IsThrottled);
                Assert.False(result.IsLocked);
            }

            [Fact]
            public void LimitReachedWithLocking_ReturnsThrottled()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    Count = 1,
                    LockDuration = TimeSpan.FromSeconds(1)
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(false);

                repo.GetThrottleCount(key, limit)
                    .Returns(1);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                CheckResult result = policy.Check(key);

                // Assert
                Assert.True(result.IsThrottled);
                Assert.False(result.IsLocked);

                repo.Received(1)
                    .SetLock(key, limit);
                repo.Received(1)
                    .RemoveThrottle(key, limit);
            }

            [Fact]
            public void NotThrottled_Increments()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    Count = 2,
                    LockDuration = TimeSpan.FromSeconds(1)
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(false);

                repo.GetThrottleCount(key, limit)
                    .Returns(1);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                policy.Check(key);

                // Assert
                repo.Received(1)
                    .AddOrIncrementWithExpiration(key, limit);
            }

            [Fact]
            public void NotThrottled_DoesNotIncrements()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    Count = 2,
                    LockDuration = TimeSpan.FromSeconds(1)
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(false);

                repo.GetThrottleCount(key, limit)
                    .Returns(1);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                policy.Check(key, false);

                // Assert
                repo.Received(0)
                    .AddOrIncrementWithExpiration(key, limit);
            }
        }

        public class Constructor
        {
            [Fact]
            public void Instantiate_EmptyLimiters()
            {
                // Arrange
                var policy = new ThrottlePolicy();

                // Act

                // Assert
                Assert.Equal(0, policy.Limiters.Count);
            }
        }

        public class IsLocked
        {
            [Fact]
            public void Locked_ReturnsTrue()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    Count = 1,
                    LockDuration = TimeSpan.FromSeconds(1)
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(true);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                CheckResult checkResult;
                bool result = policy.IsLocked(key, out checkResult);

                // Assert
                Assert.True(result);
            }
        }

        public class IsThrottled
        {
            [Fact]
            public void Throttled_ReturnsTrue()
            {
                // Arrange
                var key = Substitute.For<IThrottleKey>();
                var limit = new Limiter
                {
                    Count = 1,
                    LockDuration = TimeSpan.FromSeconds(1)
                };

                var repo = Substitute.For<IThrottleRepository>();
                repo.LockExists(key, limit)
                    .Returns(false);

                repo.GetThrottleCount(key, limit)
                    .Returns(1);

                var policy = new ThrottlePolicy(repo);
                policy.Limiters.Add(limit);

                // Act
                CheckResult checkResult;
                bool result = policy.IsThrottled(key, out checkResult);

                // Assert
                Assert.True(result);
            }
        }

        public class PerPeriodSetters
        {
            [Fact]
            public void PerSecondMethod()
            {
                // Arrange/Act
                var policy = new ThrottlePolicy
                {
                    PerSecond = 10
                };

                // Assert
                Limiter limiter = policy.Limiters.First();
                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromSeconds(1), limiter.Period);

                // Testing the getter
                Assert.Equal(limiter.Count, policy.PerSecond);
            }

            [Fact]
            public void PerMinuteMethod()
            {
                // Arrange/Act
                var policy = new ThrottlePolicy
                {
                    PerMinute = 10
                };

                // Assert
                Limiter limiter = policy.Limiters.First();
                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromMinutes(1), limiter.Period);

                // Testing the getter
                Assert.Equal(limiter.Count, policy.PerMinute);
            }

            [Fact]
            public void PerHourMethod()
            {
                // Arrange/Act
                var policy = new ThrottlePolicy
                {
                    PerHour = 10
                };

                // Assert
                Limiter limiter = policy.Limiters.First();
                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromHours(1), limiter.Period);

                // Testing the getter
                Assert.Equal(limiter.Count, policy.PerHour);
            }

            [Fact]
            public void PerDayMethod()
            {
                // Arrange/Act
                var policy = new ThrottlePolicy
                {
                    PerDay = 10
                };

                // Assert
                Limiter limiter = policy.Limiters.First();
                Assert.Equal(10, limiter.Count);
                Assert.Equal(TimeSpan.FromDays(1), limiter.Period);

                // Testing the getter
                Assert.Equal(limiter.Count, policy.PerDay);
            }
        }
    }
}