namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RetireAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RetireAddressRequest>
    {
        public RetireAddressRequest Request { get; init; }
    }
}
