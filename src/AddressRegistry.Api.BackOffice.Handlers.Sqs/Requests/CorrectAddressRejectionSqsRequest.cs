namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressRejectionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressRejectionBackOfficeRequest>
    {
        public CorrectAddressRejectionBackOfficeRequest Request { get; init; }
    }
}
