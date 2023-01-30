namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressRetirementSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressRetirementRequest>
    {
        public CorrectAddressRetirementRequest Request { get; init; }
    }
}
