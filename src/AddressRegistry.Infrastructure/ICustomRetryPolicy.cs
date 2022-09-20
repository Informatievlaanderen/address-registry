namespace AddressRegistry.Infrastructure
{
    using System;
    using System.Threading.Tasks;

    public interface ICustomRetryPolicy
    {
        Task Retry(Func<Task> functionToRetry);
    }
}
