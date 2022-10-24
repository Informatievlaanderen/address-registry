namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectPositionSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectPositionRequest>
    {
        public int PersistentLocalId { get; set; }

        public BackOfficeCorrectPositionRequest Request { get; set; }
    }
}
