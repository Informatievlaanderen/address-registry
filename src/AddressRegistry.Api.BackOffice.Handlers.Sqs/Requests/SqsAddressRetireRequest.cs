namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressRetireRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRetireRequest>
    {
        public AddressBackOfficeRetireRequest Request { get; set; }
    }
}
