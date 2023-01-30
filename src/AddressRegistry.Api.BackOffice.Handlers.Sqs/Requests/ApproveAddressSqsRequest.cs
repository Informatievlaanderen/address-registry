namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ApproveAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<ApproveAddressRequest>
    {
        public ApproveAddressRequest Request { get; init; }
    }
}
