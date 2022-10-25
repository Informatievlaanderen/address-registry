namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RejectAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RejectAddressBackOfficeRequest>
    {
        public RejectAddressBackOfficeRequest Request { get; init; }
    }
}
