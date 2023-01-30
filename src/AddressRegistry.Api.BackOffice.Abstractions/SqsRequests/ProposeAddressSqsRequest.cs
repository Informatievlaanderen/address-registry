namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeAddressRequest>
    {
        public ProposeAddressRequest Request { get; init; }
    }
}
