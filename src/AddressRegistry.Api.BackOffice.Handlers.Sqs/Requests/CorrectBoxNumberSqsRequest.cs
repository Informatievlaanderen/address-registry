namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectBoxNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectBoxNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public BackOfficeCorrectBoxNumberRequest Request { get; set; }
    }
}
