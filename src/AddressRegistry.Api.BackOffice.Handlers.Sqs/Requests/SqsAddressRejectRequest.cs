namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressRejectRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRejectRequest>
    {
        public AddressBackOfficeRejectRequest Request { get; set; }
    }
}
