namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectHouseNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectHouseNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public BackOfficeCorrectHouseNumberRequest Request { get; set; }
    }
}
