namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ApproveAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<ApproveAddressBackOfficeRequest>
    {
        public ApproveAddressBackOfficeRequest Request { get; init; }
    }
}
