namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class ReaddressSqsRequest : SqsRequest, IHasBackOfficeRequest<ReaddressRequest>
    {
        public ReaddressRequest Request { get; init; }
    }
}
