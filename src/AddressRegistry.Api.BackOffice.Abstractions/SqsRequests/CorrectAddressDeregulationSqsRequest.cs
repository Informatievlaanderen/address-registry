namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressDeregulationSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressDeregulationRequest>
    {
        public CorrectAddressDeregulationRequest Request { get; init; }
    }
}
