namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressRegularizationSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressRegularizationRequest>
    {
        public CorrectAddressRegularizationRequest Request { get; init; }
    }
}
