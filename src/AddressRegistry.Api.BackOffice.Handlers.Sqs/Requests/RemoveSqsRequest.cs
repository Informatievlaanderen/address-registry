namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RemoveSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeRemoveRequest>
    {
        public BackOfficeRemoveRequest Request { get; set; }
    }
}
