namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RegularizeAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RegularizeAddressRequest>
    {
        public RegularizeAddressRequest Request { get; init; }
    }
}
