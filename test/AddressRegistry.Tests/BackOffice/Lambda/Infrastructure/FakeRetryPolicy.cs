namespace AddressRegistry.Tests.BackOffice.Lambda.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;

    internal class FakeRetryPolicy : ICustomRetryPolicy
    {
        public Task Retry(Func<Task> functionToRetry)
        {
            return functionToRetry();
        }
    }
}
