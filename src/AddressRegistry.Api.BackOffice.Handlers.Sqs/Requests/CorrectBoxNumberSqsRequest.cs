namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectBoxNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectBoxNumberBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectBoxNumberBackOfficeRequest Request { get; set; }
    }
}
