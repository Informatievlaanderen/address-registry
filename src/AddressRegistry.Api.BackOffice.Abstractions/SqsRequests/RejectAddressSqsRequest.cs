namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RejectAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RejectAddressRequest>
    {
        public RejectAddressRequest Request { get; init; }
    }
}
