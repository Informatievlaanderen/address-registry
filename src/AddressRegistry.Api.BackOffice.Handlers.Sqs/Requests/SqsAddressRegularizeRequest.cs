namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressRegularizeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRegularizeRequest>
    {
        public AddressBackOfficeRegularizeRequest Request { get; set; }
    }
}
