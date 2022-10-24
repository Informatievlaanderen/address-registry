namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectRejectionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectRejectionBackOfficeRequest>
    {
        public CorrectRejectionBackOfficeRequest Request { get; set; }
    }
}
