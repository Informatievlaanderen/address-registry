namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RemoveSqsRequest : SqsRequest, IHasBackOfficeRequest<RemoveBackOfficeRequest>
    {
        public RemoveBackOfficeRequest Request { get; set; }
    }
}
