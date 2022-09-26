namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Infrastructure
{
    using AddressRegistry.Infrastructure;
    using Microsoft.Data.SqlClient;
    using Polly;
    using Polly.Retry;

    public sealed class LambdaHandlerRetryPolicy : ICustomRetryPolicy
    {
        private readonly AsyncRetryPolicy _policy;

        public LambdaHandlerRetryPolicy(int maxRetryCount, int delaySeconds)
        {
            _policy = Policy
                .Handle<SqlException>(ex => TransientSqlErrors.Errors.Contains(ex.Number))
                .WaitAndRetryAsync(
                    maxRetryCount,
                    currentRetryCount => TimeSpan
                        .FromSeconds(delaySeconds)
                        .Multiply(currentRetryCount));
        }

        public async Task Retry(Func<Task> functionToRetry)
        {
            await _policy.ExecuteAsync(functionToRetry);
        }
    }
}
