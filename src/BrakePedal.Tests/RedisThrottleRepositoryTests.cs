using NSubstitute;
using StackExchange.Redis;
using BrakePedal.Redis;
using Xunit;

namespace BrakePedal.Tests
{
    public class RedisThrottleRepositoryTests
    {
        public class AddOrIncrementWithExpirationMethod
        {
            [Fact]
            public void IncrementReturnsOne_ExpireKey()
            {
                // Arrange
                var key = new SimpleThrottleKey("test", "key");
                Limiter limiter = new Limiter().Limit(1).Over(10);
                var db = Substitute.For<IDatabase>();
                var repository = new RedisThrottleRepository(db);
                string id = repository.CreateThrottleKey(key, limiter);

                db
                    .StringIncrement(id)
                    .Returns(1);

                // Act
                repository.AddOrIncrementWithExpiration(key, limiter);

                // Assert
                db
                    .Received(1)
                    .StringIncrement(id);

                db
                    .Received(1)
                    .KeyExpire(id, limiter.Period);
            }
        }

        public class GetThrottleCountMethod
        {
            [Fact]
            public void KeyDoesNotExist_ReturnsNull()
            {
                // Arrange
                var key = new SimpleThrottleKey("test", "key");
                Limiter limiter = new Limiter().Limit(1).Over(1);
                var db = Substitute.For<IDatabase>();
                var repository = new RedisThrottleRepository(db);
                string id = repository.CreateThrottleKey(key, limiter);

                db
                    .StringGet(id)
                    .Returns((long?)null);

                // Act
                long? result = repository.GetThrottleCount(key, limiter);

                // Assert
                Assert.Null(result);
            }

            [Fact]
            public void KeyExists_ReturnsParsedValue()
            {
                // Arrange
                var key = new SimpleThrottleKey("test", "key");
                Limiter limiter = new Limiter().Limit(1).Over(1);
                var db = Substitute.For<IDatabase>();
                var repository = new RedisThrottleRepository(db);
                string id = repository.CreateThrottleKey(key, limiter);

                db
                    .StringGet(id)
                    .Returns((RedisValue)"10");

                // Act
                long? result = repository.GetThrottleCount(key, limiter);

                // Assert
                Assert.Equal(10, result);
            }
        }

        public class LockExistsMethod
        {
            [Theory]
            [InlineData(true, true)]
            [InlineData(false, false)]
            public void KeyExists_ReturnsTrue(bool keyExists, bool expected)
            {
                // Arrange
                var key = new SimpleThrottleKey("test", "key");
                Limiter limiter = new Limiter().Limit(1).Over(1).LockFor(1);
                var db = Substitute.For<IDatabase>();
                var repository = new RedisThrottleRepository(db);
                string id = repository.CreateLockKey(key, limiter);

                db
                    .KeyExists(id)
                    .Returns(keyExists);

                // Act
                bool result = repository.LockExists(key, limiter);

                // Assert
                Assert.Equal(expected, result);
            }
        }

        public class RemoveThrottleMethod
        {
            [Fact]
            public void RemoveThrottle()
            {
                // Arrange
                var key = new SimpleThrottleKey("test", "key");
                Limiter limiter = new Limiter().Limit(1).Over(1);
                var db = Substitute.For<IDatabase>();
                var repository = new RedisThrottleRepository(db);
                string id = repository.CreateThrottleKey(key, limiter);

                // Act
                repository.RemoveThrottle(key, limiter);

                // Assert
                db
                    .Received(1)
                    .KeyDelete(id);
            }
        }

        public class SetLockMethod
        {
            [Fact]
            public void SetLock()
            {
                // Arrange
                var key = new SimpleThrottleKey("test", "key");
                Limiter limiter = new Limiter().Limit(1).Over(1).LockFor(1);
                var db = Substitute.For<IDatabase>();
                var repository = new RedisThrottleRepository(db);
                string id = repository.CreateLockKey(key, limiter);
                var transaction = Substitute.For<ITransaction>();

                db
                    .CreateTransaction()
                    .Returns(transaction);

                // Act
                repository.SetLock(key, limiter);

                // Assert
                transaction
                    .Received(1)
                    .StringIncrementAsync(id);

                transaction
                    .Received(1)
                    .KeyExpireAsync(id, limiter.LockDuration);

                transaction
                    .Received(1)
                    .Execute();
            }
        }
    }
}