namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressRetirementSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressRetirementBackOfficeRequest>
    {
        public CorrectAddressRetirementBackOfficeRequest Request { get; init; }
    }
}
