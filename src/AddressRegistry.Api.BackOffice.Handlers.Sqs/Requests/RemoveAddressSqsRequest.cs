namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RemoveAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RemoveAddressRequest>
    {
        public RemoveAddressRequest Request { get; init; }
    }
}
