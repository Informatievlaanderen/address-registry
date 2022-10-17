namespace AddressRegistry.Tests.BackOffice.Lambda.Infrastructure
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using FluentAssertions;
    using Microsoft.Data.SqlClient;
    using Xunit;

    public class LambdaHandlerRetryPolicyTests
    {
        [Theory]
        [InlineData(926)]
        [InlineData(4060)]
        [InlineData(40197)]
        [InlineData(40501)]
        [InlineData(40549)]
        [InlineData(40550)]
        [InlineData(40613)]
        [InlineData(49918)]
        [InlineData(49919)]
        [InlineData(49920)]
        [InlineData(4221)]
        [InlineData(615)]
        public async Task RetryOnSpecificSqlExceptionErrors(int errorNumber)
        {
            await AssertExpectedRetryCount(() => throw SqlExceptionMocker.NewSqlException(errorNumber), 2, 2);
        }

        [Fact]
        public async Task DoesNotRetryOnAggregateNotFoundException()
        {
            await AssertExpectedRetryCount(() => throw new AggregateNotFoundException(string.Empty, typeof(string)), 2, 0);
        }


        [Fact]
        public async Task DoesNotRetryOnIfMatchHeaderValueMismatchException()
        {
            await AssertExpectedRetryCount(() => throw new IfMatchHeaderValueMismatchException(null), 2, 0);
        }

        [Fact]
        public async Task DoesNotRetryOnSqlConnectionFailure()
        {
            await AssertExpectedRetryCount(() => new SqlConnection(@"Data Source=.;Database=GUARANTEED_TO_FAIL;Connection Timeout=1").Open(), 2, 0);
        }

        private async Task AssertExpectedRetryCount(Action throwExceptionMethod, int maxRetry, int expectedRetry)
        {
            var retryCounter = -1; // First execution is not part of the retry count

            var sut = new LambdaHandlerRetryPolicy(maxRetry, 0);

            try
            {
                // Act
                await sut.Retry(() =>
                {
                    retryCounter++;
                    throwExceptionMethod();
                    return Task.CompletedTask;
                });
            }
            catch (Exception) { }

            // Assert
            retryCounter.Should().Be(expectedRetry);
        }
    }

    class SqlExceptionMocker
    {
        private static T Construct<T>(params object[] p)
        {
            var ctor = typeof(T)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(c => c.GetParameters().Length == p.Length);

            return (T)ctor.Invoke(p);
        }

        public static SqlException NewSqlException(int errorNumber)
        {
            var collection = Construct<SqlErrorCollection>();
            var error = Construct<SqlError>(errorNumber, (byte)2, (byte)3, "server name", "This is a Mock-SqlException", "proc", 100, new Exception());

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(collection, new object[] { error });


            var e = typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.ExplicitThis, new[] { typeof(SqlErrorCollection), typeof(string) }, new ParameterModifier[] { })
                ?.Invoke(null, new object[] { collection, string.Empty }) as SqlException;

            return e;
        }
    }
}

