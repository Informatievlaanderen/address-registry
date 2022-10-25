namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RemoveAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<RemoveAddressBackOfficeRequest>
    {
        public RemoveAddressBackOfficeRequest Request { get; init; }
    }
}
