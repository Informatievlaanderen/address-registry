namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RemoveAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RemoveAddressRequest>
    {
        public RemoveAddressRequest Request { get; init; }
    }
}
