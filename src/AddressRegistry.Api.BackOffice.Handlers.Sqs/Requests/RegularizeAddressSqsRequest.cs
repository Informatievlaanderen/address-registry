namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RegularizeAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RegularizeAddressBackOfficeRequest>
    {
        public RegularizeAddressBackOfficeRequest Request { get; init; }
    }
}
