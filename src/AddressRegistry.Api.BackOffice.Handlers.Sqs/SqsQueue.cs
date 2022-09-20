namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

    public class SqsQueue : ISqsQueue
    {
        private readonly SqsOptions _sqsOptions;
        private readonly string _sqsQueueName = $"{nameof(AddressRegistry)}.{nameof(Api)}.{nameof(BackOffice)}";

        public SqsQueue(SqsOptions sqsOptions)
        {
            _sqsOptions = sqsOptions;
        }

        public async Task<bool> Copy<T>(T message, SqsQueueOptions sqsQueueOptions, CancellationToken cancellationToken) where T : class
        {
            return await Sqs.CopyToQueue(_sqsOptions, _sqsQueueName, message, sqsQueueOptions, cancellationToken);
        }
    }
}
