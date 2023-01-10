namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressDeregularizationSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressDeregularizationBackOfficeRequest>
    {
        public CorrectAddressDeregularizationBackOfficeRequest Request { get; init; }
    }
}
