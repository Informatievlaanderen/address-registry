namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressRegularizationSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressRegularizationBackOfficeRequest>
    {
        public CorrectAddressRegularizationBackOfficeRequest Request { get; init; }
    }
}
