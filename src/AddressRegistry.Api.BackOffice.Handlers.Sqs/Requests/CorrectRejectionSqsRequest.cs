namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectRejectionSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectAddressFromRejectedRequest>
    {
        public BackOfficeCorrectAddressFromRejectedRequest Request { get; set; }
    }
}
