namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class DeregulateAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<DeregulateAddressRequest>
    {
        public DeregulateAddressRequest Request { get; init; }
    }
}
