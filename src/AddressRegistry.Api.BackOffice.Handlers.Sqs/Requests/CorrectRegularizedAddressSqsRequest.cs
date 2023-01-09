namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectRegularizedAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectRegularizedAddressBackOfficeRequest>
    {
        public CorrectRegularizedAddressBackOfficeRequest Request { get; init; }
    }
}
