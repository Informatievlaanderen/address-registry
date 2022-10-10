namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

    public sealed class SqsQueue : ISqsQueue
    {
        private readonly SqsOptions _sqsOptions;
        private readonly string _queueUrl;

        public SqsQueue(SqsOptions sqsOptions, string queueUrl)
        {
            _sqsOptions = sqsOptions;
            _queueUrl = queueUrl;
        }

        public async Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
        {
            return await Sqs.CopyToQueue(_sqsOptions, _queueUrl, message, queueOptions, cancellationToken);
        }
    }
}
