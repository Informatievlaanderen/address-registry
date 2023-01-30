namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RetireAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RetireAddressRequest>
    {
        public RetireAddressRequest Request { get; init; }
    }
}
