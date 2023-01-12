namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressDeregulationSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressDeregulationBackOfficeRequest>
    {
        public CorrectAddressDeregulationBackOfficeRequest Request { get; init; }
    }
}
