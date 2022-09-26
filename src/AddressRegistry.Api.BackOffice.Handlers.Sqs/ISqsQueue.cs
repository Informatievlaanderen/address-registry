namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISqsQueue
    {
        Task<bool> Copy<T>(
            T message,
            SqsQueueOptions queueOptions,
            CancellationToken cancellationToken)
            where T : class;
    }
}
