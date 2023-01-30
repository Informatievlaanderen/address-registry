namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ApproveAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<ApproveAddressRequest>
    {
        public ApproveAddressRequest Request { get; init; }
    }
}
