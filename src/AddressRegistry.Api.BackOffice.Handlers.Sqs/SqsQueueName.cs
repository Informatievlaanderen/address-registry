namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    internal static class SqsQueueName
    {
        public const string Value = $"{nameof(AddressRegistry)}.{nameof(Api)}.{nameof(BackOffice)}";
    }
}
