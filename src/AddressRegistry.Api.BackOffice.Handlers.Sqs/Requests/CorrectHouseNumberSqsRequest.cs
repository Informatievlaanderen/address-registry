namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectHouseNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectHouseNumberBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectHouseNumberBackOfficeRequest Request { get; set; }
    }
}
