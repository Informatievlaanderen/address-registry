namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangePositionSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangePositionBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public ChangePositionBackOfficeRequest Request { get; set; }
    }
}
