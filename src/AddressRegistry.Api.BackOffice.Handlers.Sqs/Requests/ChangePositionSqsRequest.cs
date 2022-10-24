namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangePositionSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeChangePositionRequest>
    {
        public int PersistentLocalId { get; set; }

        public BackOfficeChangePositionRequest Request { get; set; }
    }
}
