namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RegularizeAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RegularizeAddressRequest>
    {
        public RegularizeAddressRequest Request { get; init; }
    }
}
