namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressRemoveRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRemoveRequest>
    {
        public AddressBackOfficeRemoveRequest Request { get; set; }
    }
}
