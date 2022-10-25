namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeAddressBackOfficeRequest>
    {
        public ProposeAddressBackOfficeRequest Request { get; init; }
    }
}
