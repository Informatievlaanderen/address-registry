namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectRetirementSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectRetirementRequest>
    {
        public BackOfficeCorrectRetirementRequest Request { get; set; }
    }
}
