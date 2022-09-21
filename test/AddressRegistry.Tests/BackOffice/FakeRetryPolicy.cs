namespace AddressRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;

    internal class FakeRetryPolicy : ICustomRetryPolicy
    {
        public Task Retry(Func<Task> functionToRetry)
        {
            return functionToRetry();
        }
    }
}
